using AutoMapper;
using AutoMapper.QueryableExtensions;
using AutoWrapper.Wrappers;
using Common;
using Common.Model;
using MakanPergi.Entities;
using MakanPergiWebApi.Interfaces.Repository;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MakanPergiWebApi.Services
{
    public class ShopService : BaseService, IShopService
    {
        private readonly IMapper _mapper;
        private readonly IFoodService _foodService;

        public ShopService(IUnitOfWork unitofWork, IMapper mapper, IFoodService foodService) : base(unitofWork)
        {
            _mapper = mapper;
            _foodService = foodService;
        }

        public async Task<IEnumerable<ShopViewModel>> GetAllShopsAsync()
        {
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();

            List<Shop> shops = await shopRepository.TableNoTracking.Include(f => f.Foods).ToListAsync();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Shop, ShopViewModel>();
                cfg.CreateMap<Food, ShopRelationViewModel>();
            }, NullLoggerFactory.Instance);

            var mapper = mapperConfig.CreateMapper();

            List<ShopViewModel> shopViewModels = mapper.Map<List<ShopViewModel>>(shops);

            return shopViewModels;
        }

        public async Task<DataTablesResponse<ShopIndexViewModel>> GetShopsDataTablesAsync(DatatablesRequest request)
        {
            IQueryable<Shop> allShops = WorkContext.Repository<Shop>().TableNoTracking;

            MapperConfiguration? mapperConfiguration = new MapperConfiguration(cfg => cfg.CreateProjection<Shop, ShopIndexViewModel>()
                                        .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region.Name)), NullLoggerFactory.Instance);

            if (!string.IsNullOrWhiteSpace(request?.Search?.Value) || !string.IsNullOrEmpty(request?.Search?.Value))
            {
                string? pattern = $"%{request.Search.Value.Trim()}%";

                // ** WE CANNOT USE THIS when doing CreateProjection.
                // overload of string.Contains(…, StringComparison) has no SQL translation
                // and EF Core doesn’t know how to turn it into a LIKE and throws the “could not be translated” error.

                // shop.Name.Contains(request.Search.Value, StringComparison.OrdinalIgnoreCase)
                allShops = allShops.Where(shop => EF.Functions.Like(shop.Name, pattern) ||
                                                  EF.Functions.Like(shop.Region.Name, pattern)
                );
            }

            int totalShopCount = await allShops.CountAsync();

            int sortColumnIndex = request.Order[0].Column;
            string sortDirection = request.Order[0].Dir ?? "asc";

            string? sortField = request.Columns[sortColumnIndex].Data;

            switch (sortField)
            {
                case "id":
                    Console.WriteLine($"Sorting by Id ({sortDirection})");
                    allShops = sortDirection == "asc"
                        ? allShops.OrderBy(c => c.Id)
                        : allShops.OrderByDescending(c => c.Id);
                    break;

                case "name":
                    Console.WriteLine($"Sorting by Name ({sortDirection})");
                    allShops = sortDirection == "asc"
                        ? allShops.OrderBy(c => c.Name)
                        : allShops.OrderByDescending(c => c.Name);
                    break;

                default:
                    Console.WriteLine($"Unknown field '{sortField}', defaulting to Name");
                    allShops = sortDirection == "asc"
                        ? allShops.OrderBy(c => c.Name)
                        : allShops.OrderByDescending(c => c.Name);
                    break;
            }

            IQueryable<Shop> paginatedShops = allShops.Skip(request.Start).Take(request.Length);

            List<ShopIndexViewModel> paginatedShopViewModel =
                await paginatedShops.ProjectTo<ShopIndexViewModel>(mapperConfiguration).ToListAsync();

            DataTablesResponse<ShopIndexViewModel> response = new DataTablesResponse<ShopIndexViewModel>
            {
                Draw = request.Draw,
                RecordsTotal = totalShopCount,
                RecordsFiltered = totalShopCount,
                Data = paginatedShopViewModel
            };

            Console.WriteLine(response.ToJsonString());

            return response;

        }

        public async Task<IEnumerable<ShopViewModel>> GetAllRegionAsync()
        {
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();

            List<Shop> shops = await shopRepository.TableNoTracking.Where( shop => shop.RegionId == null).ToListAsync();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Shop, ShopViewModel>();
            }, NullLoggerFactory.Instance);

            var mapper = mapperConfig.CreateMapper();

            List<ShopViewModel> shopViewModels = mapper.Map<List<ShopViewModel>>(shops);

            return shopViewModels;
        }

        public async Task<IEnumerable<TreeViewNodeModel>> GetShopsTreeViewAsync()
        {
            List<Shop> shops = await WorkContext.Repository<Shop>().TableNoTracking
                                                                   .ToListAsync();

            List<TreeViewNodeModel> shopsTreeViewNodes = _mapper.Map<List<TreeViewNodeModel>>(shops);

            return shopsTreeViewNodes;
        }

        public async Task<IEnumerable<TreeViewNodeModel>> GetShopsTreeViewForSelectedFoodAsync(Guid id)
        {
            List<Shop> shops = await WorkContext.Repository<Shop>().TableNoTracking
                                                                   .ToListAsync();

            List<TreeViewNodeModel> shopsNodeModel = _mapper.Map<List<TreeViewNodeModel>>(shops);

            FoodViewModel foodViewModel = await _foodService.GetFoodByIdAsync(id);

            List<Guid> existingFoodShopIds = foodViewModel.ShopsOffered.Select( shop => shop.Id ).ToList();

            shopsNodeModel.ForEach(categoryNodeModel =>
            {
                if (existingFoodShopIds.Contains(Guid.Parse(categoryNodeModel.Id)))
                {
                    categoryNodeModel.State.Selected = true;
                }
            });

            return shopsNodeModel;
        }

        public async Task<ShopViewModel> GetShopByIdAsync(Guid id)
        {
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();
            Shop? existingShop = await shopRepository.TableNoTracking.FirstOrDefaultAsync(c => c.Id == id);

            if (existingShop is null)
            {
                throw new ApiException("No shop found", StatusCodes.Status404NotFound);
            }

            var mapper = new MapperConfiguration(mapConfig =>
            {
                mapConfig.CreateMap<Shop, ShopViewModel>()
                         .ForMember(dest => dest.Foods, opt => opt.Ignore());
            }, NullLoggerFactory.Instance);

            ShopViewModel shopViewModel = mapper.CreateMapper().Map<ShopViewModel>(existingShop);

            return shopViewModel;

        }

        public async Task AddNewShopAsync(ShopDataModel shopDataModel)
        {
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            var mapper = new MapperConfiguration(mapConfig =>
            {

                mapConfig.CreateMap<ShopDataModel, Shop>()
                .ForMember(dest => dest.Foods,
                            opt => opt.MapFrom(src => foodRepository.Table.Where(f => src.Foods.Contains(f.Id))))
                .ForMember(dest => dest.Region, 
                           opt => opt.MapFrom(src => shopDataModel.RegionId == null ? 
                                                     null : 
                                                     shopRepository.Table.Where(s => s.Id == shopDataModel.RegionId).FirstOrDefault()
                                                     )
                           );

            }, NullLoggerFactory.Instance);

            Shop shop = mapper.CreateMapper().Map<Shop>(shopDataModel);

            await shopRepository.AddAsync(shop);
            await WorkContext.CompleteAsync();

        }

        public async Task UpdateShopAsync(ShopDataModel shopDataModel)
        {
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            Shop? existingShop = await shopRepository.Table.Where(c => c.Id == shopDataModel.Id).FirstOrDefaultAsync();

            if (existingShop is null)
            {
                throw new ApiException($"No Shop with Id: {shopDataModel.Id} found", StatusCodes.Status404NotFound);
            }

            var mapper = new MapperConfiguration(mapConfig =>
            {

                mapConfig.CreateMap<ShopDataModel, Shop>()
                .ForMember(dest => dest.Foods,
                            opt => opt.MapFrom(src => foodRepository.Table.Where(f => src.Foods.Contains(f.Id))));

            }, NullLoggerFactory.Instance);

            Shop shop = mapper.CreateMapper().Map(shopDataModel, existingShop);

            shopRepository.Update(shop);
            await WorkContext.CompleteAsync();
        }

        public async Task DeleteShopAsync(Guid id)
        {
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();

            Shop? shop = await shopRepository.Table.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (shop is null)
            {
                throw new ApiException($"No Shop with Id: {id} found", StatusCodes.Status404NotFound);
            }

            shopRepository.Remove(shop);
            await WorkContext.CompleteAsync();
        }
    }
}

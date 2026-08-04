using AutoMapper;
using AutoWrapper.Wrappers;
using Common;
using MakanPergi.Entities;
using MakanPergi.Entities.Views;
using MakanPergiWebApi.Interfaces.Repository;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace MakanPergiWebApi.Services
{
    public class FoodService : BaseService, IFoodService
    {
        private readonly IMapper _mapper;

        public FoodService(IUnitOfWork unitofWork, IMapper mapper) : base(unitofWork)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<FoodViewModel>> GetAllFoodsAsync()
        {
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            List<Food> foods = await foodRepository.TableNoTracking.Include(f => f.Cuisine)
                                                                   .Include(f => f.Categories)
                                                                   .Include(f => f.ShopsOffered)
                                                                   .ToListAsync();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Food, FoodViewModel>();
                cfg.CreateMap<Category, CategoryRelationViewModel>();
                cfg.CreateMap<Shop, ShopRelationViewModel>();
                cfg.CreateMap<Cuisine, CuisineViewModel>();
            }, NullLoggerFactory.Instance);

            var mapper = mapperConfig.CreateMapper();

            List<FoodViewModel> foodViewModels = mapper.Map<List<FoodViewModel>>(foods);

            return foodViewModels;
        }

        public async Task<IEnumerable<VW_FoodFullView>> GetAllFoodFullViewAsync()
        {
            return await WorkContext.Repository<VW_FoodFullView>().TableNoTracking.ToListAsync();
        }

        public async Task<FoodViewModel> GetFoodByIdAsync(Guid id)
        {
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            Food? food = await foodRepository.TableNoTracking.Include(f => f.Cuisine)
                                                                   .Include(f => f.Categories)
                                                                   .Include(f => f.ShopsOffered)
                                                                   .Where( f => f.Id == id).FirstOrDefaultAsync();

            if (food is null)
            {
                throw new ApiException($"No Food with Id: {id} found", StatusCodes.Status404NotFound);
            }



            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Cuisine, CuisineViewModel>();
                cfg.CreateMap<Category, CategoryRelationViewModel>();
                cfg.CreateMap<Shop, ShopRelationViewModel>();
                cfg.CreateMap<Food, FoodViewModel>();
            }, NullLoggerFactory.Instance);

            FoodViewModel foodViewModel = mapperConfig.CreateMapper().Map<FoodViewModel>(food);

            return foodViewModel;
        }

        public async Task<FoodEditViewModel> GetFoodEditViewModelByIdAsync(Guid id)
        {
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            Food? food = await foodRepository.TableNoTracking.Include(f => f.Cuisine)
                                                                   .Include(f => f.Categories)
                                                                   .Include(f => f.ShopsOffered)
                                                                   .Where(f => f.Id == id).FirstOrDefaultAsync();

            if (food is null)
            {
                throw new ApiException($"No Food with Id: {id} found", StatusCodes.Status404NotFound);
            }

            MapperConfiguration? mapperConfig = new MapperConfiguration(cfg => 
            {
                cfg.CreateMap<Food, FoodEditViewModel>()
                   .ForMember(dest => dest.CuisineId, opt => opt.MapFrom(src => src.Cuisine.Id))
                   .ForMember(dest => dest.CategoryNames, opt => opt.Ignore())
                   .ForMember(dest => dest.ShopNames, opt => opt.Ignore());
            }, NullLoggerFactory.Instance);

            FoodEditViewModel foodEditViewModel = mapperConfig.CreateMapper().Map<FoodEditViewModel>(food);

            return foodEditViewModel;
        }

        public async Task AddNewFoodAsync(FoodDataModel foodDataModel)
        {
            IRepository<Category>? categoryRepository = WorkContext.Repository<Category>();
            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            var mapper = new MapperConfiguration(mapConfig =>
            {
                //mapConfig.CreateMap<Guid, Cuisine>()
                //.ConstructUsing( id => cuisineRepository.Table.First(c => c.Id == id));

                mapConfig.CreateMap<FoodDataModel, Food>()
                .ForMember(dest => dest.Cuisine, opt => opt.MapFrom(src => cuisineRepository.Table.FirstOrDefault(c => c.Id == src.CuisineId)))
                    .BeforeMap((src, dest) => Console.WriteLine($"BeforeMap: CuisineId = {src.CuisineId}"))
                    .AfterMap((src, dest) => Console.WriteLine($"AfterMap: Cuisine = {dest.Cuisine?.Name ?? "null"}"))
                .ForMember(dest => dest.Categories,
                            opt => opt.MapFrom(src => categoryRepository.Table.Where(f => src.Categories.Contains(f.Id))))
                .ForMember(dest => dest.ShopsOffered,
                            opt => opt.MapFrom(src => shopRepository.Table.Where(f => src.ShopsOffered.Contains(f.Id))));

            }, NullLoggerFactory.Instance);

            Food food = mapper.CreateMapper().Map<Food>(foodDataModel);

            await foodRepository.AddAsync(food);
            await WorkContext.CompleteAsync();

        }

        public async Task UpdateFoodAsync(FoodDataModel foodDataModel)
        {
            IRepository<Category>? categoryRepository = WorkContext.Repository<Category>();
            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();
            IRepository<Shop>? shopRepository = WorkContext.Repository<Shop>();
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            Food? existingFood = await foodRepository.Table.Where(c => c.Id == foodDataModel.Id).FirstOrDefaultAsync();

            if (existingFood is null)
            {
                throw new ApiException($"No Food with Id: {foodDataModel.Id} found", StatusCodes.Status404NotFound);
            }

            var mapper = new MapperConfiguration(mapConfig =>
            {
                //mapConfig.CreateMap<Guid, Cuisine>()
                //.ConstructUsing(id => cuisineRepository.Table.First(c => c.Id == id));

                mapConfig.CreateMap<FoodDataModel, Food>()
                .ForMember(dest => dest.Cuisine, opt => opt.MapFrom(src => cuisineRepository.Table.FirstOrDefault(c => c.Id == src.CuisineId)))
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.ShopsOffered, opt => opt.Ignore());
            }, NullLoggerFactory.Instance);

            mapper.CreateMapper().Map(foodDataModel, existingFood);

            Console.WriteLine($"existing food: {existingFood.ToJsonString()}");

            foodRepository.Update(existingFood);
            await UpdateFoodCategoryForFoodAsync(foodDataModel.Id, foodDataModel.Categories.ToList());
            await UpdateFoodFromShopForFoodAsync(foodDataModel.Id, foodDataModel.ShopsOffered.Distinct().ToList());
            await WorkContext.CompleteAsync();
        }

        public async Task DeleteFoodAsync(Guid id)
        {
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            Food? food = await foodRepository.Table.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (food is null)
            {
                throw new ApiException($"No Food with Id: {id} found", StatusCodes.Status404NotFound);
            }

            foodRepository.Remove(food);
            await WorkContext.CompleteAsync();
        }

        private async Task UpdateFoodCategoryForFoodAsync(Guid existingFoodId, 
                                                          List<Guid> expectedFoodCategoryGuids)
        {
            IRepository<FoodCategory> foodCategoryRepository = WorkContext.Repository<FoodCategory>();

            List<FoodCategory> existingFoodCategory =
                await foodCategoryRepository.Table.Where(fc => fc.FoodId == existingFoodId).ToListAsync();

            foreach (Guid guid in expectedFoodCategoryGuids) 
            {
                if (!existingFoodCategory.Any( existingfc => existingfc.CategoryId == guid))
                {
                    await foodCategoryRepository.AddAsync(new FoodCategory
                    {
                        FoodId = existingFoodId,
                        CategoryId = guid
                    });
                }
            }

            foreach (FoodCategory category in existingFoodCategory)
            {
                if (!expectedFoodCategoryGuids.Contains(category.CategoryId))
                {
                    foodCategoryRepository.Remove(category);
                }
            }
        }

        private async Task UpdateFoodFromShopForFoodAsync(Guid existingFoodId,
                                                          List<Guid> expectedFoodFromShopGuids)
        {
            IRepository<FoodFromShop> foodFromShopRepository = WorkContext.Repository<FoodFromShop>();

            List<FoodFromShop> existingFoodFromShop =
                await foodFromShopRepository.Table.Where(fc => fc.FoodId == existingFoodId).ToListAsync();

            foreach (Guid guid in expectedFoodFromShopGuids)
            {
                if (!existingFoodFromShop.Any(existingfc => existingfc.ShopId == guid))
                {
                    await foodFromShopRepository.AddAsync(new FoodFromShop
                    {
                        FoodId = existingFoodId,
                        ShopId = guid
                    });
                }
            }

            foreach (FoodFromShop category in existingFoodFromShop)
            {
                if (!expectedFoodFromShopGuids.Contains(category.ShopId))
                {
                    foodFromShopRepository.Remove(category);
                }
            }
        }
    }
}

using AutoMapper;
using AutoWrapper.Wrappers;
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
    public class CategoryService : BaseService, ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly IFoodService _foodService;

        public CategoryService(IUnitOfWork unitofWork, IMapper mapper, IFoodService foodService) : base(unitofWork)
        {
            _mapper = mapper;
            _foodService = foodService;
        }

        public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
        {
            IRepository<Category>? categoryRepository = WorkContext.Repository<Category>();

            List<Category> categories = await categoryRepository.TableNoTracking.Include( c => c.Foods)
                                                                                .ThenInclude(f => f.Cuisine).ToListAsync();

            var mapper = new MapperConfiguration(mapConfig =>
            {
                mapConfig.CreateMap<Category, CategoryViewModel>();
                mapConfig.CreateMap<Food, FoodRelationViewModel>();
                mapConfig.CreateMap<Cuisine, CuisineViewModel>();
            }, NullLoggerFactory.Instance);

            List<CategoryViewModel> categoryViewModels = mapper.CreateMapper().Map<List<CategoryViewModel>>(categories);

            return categoryViewModels;
        }

        public async Task<IEnumerable<TreeViewNodeModel>> GetCategoriesTreeViewAsync()
        {
            List<Category> categories = await WorkContext.Repository<Category>()
                                                         .TableNoTracking.ToListAsync();

            List<TreeViewNodeModel> categoriesNodeModel = _mapper.Map<List<TreeViewNodeModel>>(categories);

            return categoriesNodeModel;
        }

        public async Task<IEnumerable<TreeViewNodeModel>> GetCategoriesTreeViewForSelectedFoodAsync(Guid id)
        {
            List<Category> categories = await WorkContext.Repository<Category>()
                                                         .TableNoTracking.ToListAsync();

            FoodViewModel foodViewModel = await _foodService.GetFoodByIdAsync(id);

            List<TreeViewNodeModel> categoriesNodeModel = _mapper.Map<List<TreeViewNodeModel>>(categories);

            List<Guid> existingFoodCategoryIds = foodViewModel.Categories.Select(c => c.Id).ToList();

            categoriesNodeModel.ForEach(categoryNodeModel =>
            {
                if (existingFoodCategoryIds.Contains(Guid.Parse(categoryNodeModel.Id)))
                {
                    categoryNodeModel.State.Selected = true;
                }
            });

            return categoriesNodeModel;
        }

        public async Task<CategoryViewModel> GetCategoryByIdAsync(Guid id)
        {
            IRepository<Category>? categoryRepository = WorkContext.Repository<Category>();
            Category? existingCategory = await categoryRepository.TableNoTracking.Include(c => c.Foods)
                                                                                 .ThenInclude(f => f.Cuisine)
                                                                                 .FirstOrDefaultAsync( c => c.Id == id);

            if (existingCategory is null)
            {
                throw new ApiException("No category found", StatusCodes.Status404NotFound);
            }

            var mapper = new MapperConfiguration(mapConfig =>
            {
                mapConfig.CreateMap<Category, CategoryViewModel>();
                mapConfig.CreateMap<Food, FoodRelationViewModel>();
                mapConfig.CreateMap<Cuisine, CuisineViewModel>();
            }, NullLoggerFactory.Instance);

            CategoryViewModel categoryViewModel = mapper.CreateMapper().Map<CategoryViewModel>(existingCategory);

            return categoryViewModel;

        }

        public async Task AddNewCategoryAsync(CategoryDataModel categoryDataModel)
        {
            IRepository<Category>? categoryRepository = WorkContext.Repository<Category>();
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            Category category = _mapper.Map<Category>(categoryDataModel);

            await categoryRepository.AddAsync(category);
            await WorkContext.CompleteAsync();

        }

        public async Task UpdateCategoryAsync(CategoryDataModel categoryDataModel)
        {
            IRepository<Category>? categoryRepository = WorkContext.Repository<Category>();
            IRepository<Food>? foodRepository = WorkContext.Repository<Food>();

            Category? existingCategory = await categoryRepository.Table.Include( c => c.Foods)
                                                                 .Where(c => c.Id == categoryDataModel.Id)
                                                                 .FirstOrDefaultAsync();

            if (existingCategory is null)
            {
                throw new ApiException("No category found", StatusCodes.Status404NotFound);
            }

            MapperConfiguration? mapper = new MapperConfiguration(mapConfig =>
            {
                mapConfig.CreateMap<CategoryDataModel, Category>()
                .ForMember(dest => dest.Foods, opt => opt.Ignore());
            }, NullLoggerFactory.Instance);


            Category category = mapper.CreateMapper().Map(categoryDataModel, existingCategory);

            categoryRepository.Update(category);
            await WorkContext.CompleteAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            IRepository<Category>? categoryRepository = WorkContext.Repository<Category>();

            Category? category = await categoryRepository.Table.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (category is null)
            {
                throw new ApiException("No category found", StatusCodes.Status404NotFound);
            }

            categoryRepository.Remove(category);
            await WorkContext.CompleteAsync();
        }
    }
}

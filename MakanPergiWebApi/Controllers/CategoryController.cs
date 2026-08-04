using AutoMapper;
using AutoWrapper.Wrappers;
using Common;
using Common.Model;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.Dtos.Category;
using MakanPergiWebApi.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MakanPergiWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        public CategoryController(IMapper mapper, ICategoryService categoryService) 
        {
            _mapper = mapper;
            _categoryService = categoryService;
        }

        //[HttpGet]
        //public async Task<ApiResponse> GetAllCategories()
        //{
        //    try
        //    {
        //        IEnumerable<CategoryViewModel> categoryDataModel = await _categoryService.GetAllCategoriesAsync();

        //        return new ApiResponse("Categories fetched successfully", categoryDataModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApiException(ex.Message);
        //    }
        //}

        [HttpPost("search")]
        public async Task<ApiResponse> GetAllCategories([FromBody] DatatablesRequest request)
        {
            try
            {

                List<CategoryViewModel> categoryViewModel = (await _categoryService.GetAllCategoriesAsync()).ToList();

                if (!string.IsNullOrWhiteSpace(request?.Search?.Value) || !string.IsNullOrEmpty(request?.Search?.Value))
                {
                    categoryViewModel = categoryViewModel.Where(category =>
                                        category.Name.Contains(request.Search.Value, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                int cuisineViewModelsCount = categoryViewModel.Count;

                // Order array, is given like this [ {Column: "" , Dir: ""}]
                // in this example, it's the basic and simplest sorting, where Order array only have 1.
                // else, we need to use foreach
                int sortColumnIndex = request.Order[0].Column;
                string sortDirection = request.Order[0].Dir ?? "asc";

                string? sortField = request.Columns[sortColumnIndex].Data;

                switch (sortField)
                {
                    case "id":
                        Console.WriteLine($"Sorting by Id ({sortDirection})");
                        categoryViewModel = sortDirection == "asc"
                            ? categoryViewModel.OrderBy(c => c.Id).ToList()
                            : categoryViewModel.OrderByDescending(c => c.Id).ToList();
                        break;

                    case "name":
                        Console.WriteLine($"Sorting by Name ({sortDirection})");
                        categoryViewModel = sortDirection == "asc"
                            ? categoryViewModel.OrderBy(c => c.Name).ToList()
                            : categoryViewModel.OrderByDescending(c => c.Name).ToList();
                        break;

                    default:
                        Console.WriteLine($"Unknown field '{sortField}', defaulting to Name");
                        categoryViewModel = sortDirection == "asc"
                            ? categoryViewModel.OrderBy(c => c.Name).ToList()
                            : categoryViewModel.OrderByDescending(c => c.Name).ToList();
                        break;
                }

                List<CategoryViewModel> selectedCategoryViewModel =
                                       categoryViewModel.Skip(request.Start).Take(request.Length).ToList();

                Console.WriteLine($"Filtered categoryViewModel - {selectedCategoryViewModel.Count}");

                DataTablesResponse<CategoryViewModel> response = new DataTablesResponse<CategoryViewModel>
                {
                    Draw = request.Draw,
                    RecordsTotal = cuisineViewModelsCount,
                    RecordsFiltered = cuisineViewModelsCount,
                    Data = selectedCategoryViewModel
                };

                return new ApiResponse("Categories fetched successfully", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Backend CategoryController - Exception occurred");
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("categoryTreeView")]
        public async Task<ApiResponse> GetCategoryTreeView()
        {
            try
            {
                List<TreeViewNodeModel> categoriesNodeModel = (await _categoryService.GetCategoriesTreeViewAsync()).ToList();

                return new ApiResponse("Categories Tree View fetched successfully", categoriesNodeModel);
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Backend GetCategoryTreeView - Exception occurred");
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("categoryTreeView/{id}")]
        public async Task<ApiResponse> GetCategoryTreeViewForSelectedFood(Guid id)
        {
            List<TreeViewNodeModel> categoryNodeModels = 
                (await _categoryService.GetCategoriesTreeViewForSelectedFoodAsync(id)).ToList();

            return new ApiResponse($"Category with related Food Id - {id} fetched successfully", categoryNodeModels);
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse> GetCategory(string id)
        {
            try
            {
                CategoryViewModel categoryDataModel = await _categoryService.GetCategoryByIdAsync(Guid.Parse(id));

                return new ApiResponse($"Category with Id {categoryDataModel.Id} fetched successfully", categoryDataModel);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResponse> AddNewCategoryAsync([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                CategoryDataModel categoryDataModel = _mapper.Map<CategoryDataModel>(createCategoryDto);
                await _categoryService.AddNewCategoryAsync(categoryDataModel);

                return new ApiResponse("Category Added Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }

        }

        [HttpPut("update")]
        public async Task<ApiResponse> EditCategoryAsync([FromBody] EditCategoryDto editCategoryDto)
        {
            try
            {
                CategoryDataModel categoryDataModel = _mapper.Map<CategoryDataModel>(editCategoryDto);
                await _categoryService.UpdateCategoryAsync(categoryDataModel);

                return new ApiResponse("Category Updated Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ApiResponse> RemoveCategoryAsync(Guid id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return new ApiResponse("Category Deleted Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }
    }
}

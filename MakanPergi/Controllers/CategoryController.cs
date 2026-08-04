using AutoMapper;
using Common;
using Common.Model;
using MakanPergi.Common;
using MakanPergi.Interfaces;
using MakanPergi.Models;
using MakanPergi.Models.Category;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace MakanPergi.Controllers
{
    [Route("[controller]")]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;

        public CategoryController(ILogger<CategoryController> logger, IMapper mapper, ICategoryService categoryService)
        {
            _logger = logger;
            _mapper = mapper;
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost("LoadCategoriesData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadCategoriesData(DatatablesRequest request)
        {
            ApiResponse<ResultWrapper<DataTablesResponse<CategoryViewModel>>> apiResponse = 
                                                                            await _categoryService.GetAllCategories(request);
            DataTablesResponse<CategoryViewModel>? categoryDataTables = null;
            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                Console.WriteLine($"Results found!");
                Console.WriteLine($"{apiResponse.Content.ToJsonString()}");
                categoryDataTables = apiResponse?.Content?.Result;
            }

            Console.WriteLine(categoryDataTables.ToJsonString());
            // DataTables expects a specific JSON shape when using server-side processing
            return Json(categoryDataTables);

        }

        [HttpPost("CreateCategory")]
        [Route("CreateCategory", Name = "category.create")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDtoRequest)
        {
            ApiResponse<ResultWrapper<int>> apiResponse = await _categoryService.AddNewCategoryAsync(createCategoryDtoRequest);

            return new ApiResponseActionResult<int>(apiResponse, true);

        }
         
        [HttpGet("GetCreateCategoryModal")]
        public IActionResult GetCreateCategoryModal()
        {
            CreateCategoryDto createCategoryDto = new CreateCategoryDto();

            return PartialView("_CreateCategoryModal", createCategoryDto);
        }

        [HttpPost("UpdateCategory")]
        [Route("UpdateCategory", Name = "category.update")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditCategory(EditCategoryDto editCategoryDtoRequest)
        {
            Console.WriteLine($"EditCategoryDto (Before calling backend): {editCategoryDtoRequest.ToJsonString()}");
            ApiResponse<ResultWrapper<int>> apiResponse = await _categoryService.EditCategory(editCategoryDtoRequest);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }

        [HttpGet("GetEditCategoryModal/{id}")]
        public async Task<IActionResult> GetEditCategoryModal(Guid id)
        {
            ApiResponse<ResultWrapper<CategoryViewModel>> apiResponse = await _categoryService.GetCategoryById(id);
            if (apiResponse == null)
            {
                return StatusCode(500, "Unexpected null response from service.");
            }

            if (!apiResponse.IsSuccessStatusCode || apiResponse.Content is null)
            {
                // Optionally log apiResponse.Message
                return StatusCode((int)apiResponse.StatusCode);
            }

            EditCategoryDto editCategoryDto = new EditCategoryDto
            {
                Id = apiResponse.Content.Result.Id,
                Name = apiResponse.Content.Result.Name
            };

            Console.WriteLine($"EditCategoryDto: {editCategoryDto.ToJsonString()}");
            return PartialView("_EditCategoryModal", editCategoryDto);

        }

        [HttpDelete("DeleteCategory/{id}")]
        [Route("DeleteCategory", Name = "category.delete")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            ApiResponse<ResultWrapper<int>> apiResponse = await _categoryService.DeleteCategory(id);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }
    }
}

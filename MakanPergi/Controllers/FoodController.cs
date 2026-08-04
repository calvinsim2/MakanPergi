using AutoMapper;
using Common;
using Common.Model;
using MakanPergi.Common;
using MakanPergi.Interfaces;
using MakanPergi.Models;
using MakanPergi.Models.Food;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace MakanPergi.Controllers
{
    [Route("[controller]")]
    public class FoodController : Controller
    {
        private readonly ILogger<FoodController> _logger;
        private readonly IMapper _mapper;
        private readonly IFoodService _foodService;
        private readonly ICuisineService _cuisineService;
        private readonly ICategoryService _categoryService;
        private readonly IShopService _shopService;

        public FoodController(ILogger<FoodController> logger, 
                              IMapper mapper, 
                              IFoodService foodService, 
                              ICuisineService cuisineService, 
                              ICategoryService categoryService,
                              IShopService shopService)
        {
            _logger = logger;
            _mapper = mapper;
            _foodService = foodService;
            _cuisineService = cuisineService;
            _categoryService = categoryService;
            _shopService = shopService;
        }

        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost("LoadFoodsData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadFoodsData(DatatablesRequest request)
        {
            ApiResponse<ResultWrapper<DataTablesResponse<FoodFullViewModel>>> apiResponse =
                                                                            await _foodService.GetAllFoods(request);
            DataTablesResponse<FoodFullViewModel>? foodDataTables = null;
            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                Console.WriteLine($"Results found!");
                Console.WriteLine($"{apiResponse.Content.ToJsonString()}");
                foodDataTables = apiResponse?.Content?.Result;
            }

            Console.WriteLine(foodDataTables.ToJsonString());
            // DataTables expects a specific JSON shape when using server-side processing
            return Json(foodDataTables);

        }

        [HttpGet("GetCreateFoodModal")]
        public async Task<IActionResult> GetCreateFoodModal()
        {
            CreateFoodDto createFoodDto = new CreateFoodDto();

            var cuisineServiceResponse = await _cuisineService.GetAllCuisineViewModel();

            ViewBag.AllCuisine = cuisineServiceResponse.IsSuccessStatusCode &&
                                 cuisineServiceResponse.Content.Result is not null ?
                                 cuisineServiceResponse.Content.Result : new List<Models.Cuisine.CuisineViewModel>();

            return PartialView("_CreateFoodModal", createFoodDto);
        }

        [HttpGet("GetCategoryTreeView")]
        public async Task<IActionResult> GetCategoryTreeView()
        {
            ApiResponse<ResultWrapper<List<TreeViewNodeModel>>> apiResponse = await _categoryService.GetCategoryTreeView();

            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                return Json(apiResponse.Content.Result);
            }

            return Json(new List<TreeViewNodeModel>());
        }

        [HttpGet("GetCategoryTreeView/{id}")]
        public async Task<IActionResult> GetCategoryTreeViewForSelectedFood(Guid id)
        {
            ApiResponse<ResultWrapper<List<TreeViewNodeModel>>> apiResponse = 
                await _categoryService.GetCategoryTreeViewForSelectedFood(id);

            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                return Json(apiResponse.Content.Result);
            }

            return Json(new List<TreeViewNodeModel>());
        }

        [HttpGet("GetShopTreeView")]
        public async Task<IActionResult> GetShopTreeView()
        {
            ApiResponse<ResultWrapper<List<TreeViewNodeModel>>> apiResponse = await _shopService.GetShopTreeView();

            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                return Json(apiResponse.Content.Result);
            }

            return Json(new List<TreeViewNodeModel>());
        }

        [HttpGet("GetShopTreeView/{id}")]
        public async Task<IActionResult> GetShopTreeViewForSelectedFood(Guid id)
        {
            ApiResponse<ResultWrapper<List<TreeViewNodeModel>>> apiResponse = 
                                await _shopService.GetShopTreeViewForSelectedFood(id);

            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                return Json(apiResponse.Content.Result);
            }

            return Json(new List<TreeViewNodeModel>());
        }

        [HttpPost("CreateFood")]
        [Route("CreateFood", Name = "food.create")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateFood(CreateFoodDto createFoodDtoRequest)
        {
            ApiResponse<ResultWrapper<int>> apiResponse = await _foodService.AddNewFood(createFoodDtoRequest);

            return new ApiResponseActionResult<int>(apiResponse, true);

        }

        [HttpGet("GetEditFoodModal/{id}")]
        public async Task<IActionResult> GetEditFoodModal(Guid id)
        {
            ApiResponse<ResultWrapper<FoodEditViewModel>> apiResponse = await _foodService.GetFoodEditViewModelById(id);
            if (apiResponse == null)
            {
                return StatusCode(500, "Unexpected null response from service.");
            }

            if (!apiResponse.IsSuccessStatusCode || apiResponse.Content is null)
            {
                // Optionally log apiResponse.Message
                return StatusCode((int)apiResponse.StatusCode);
            }

            var cuisineServiceResponse = await _cuisineService.GetAllCuisineViewModel();

            ViewBag.AllCuisine = cuisineServiceResponse.IsSuccessStatusCode &&
                                 cuisineServiceResponse.Content.Result is not null ?
                                 cuisineServiceResponse.Content.Result : new List<Models.Cuisine.CuisineViewModel>();

            Console.WriteLine($"FoodEditViewModel: {apiResponse.Content.Result.ToJsonString()}");
            return PartialView("_EditFoodModal", apiResponse.Content.Result);

        }

        [HttpPost("UpdateFood")]
        [Route("UpdateFood", Name = "food.edit")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditFood(FoodEditViewModel foodEditViewModel)
        {
            Console.WriteLine($"FoodEditViewModel (Before calling backend): {foodEditViewModel.ToJsonString()}");
            ApiResponse<ResultWrapper<int>> apiResponse = await _foodService.EditFood(foodEditViewModel);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }

        [HttpDelete("DeleteFood/{id}")]
        [Route("DeleteFood", Name = "food.delete")]
        public async Task<IActionResult> DeleteFood(Guid id)
        {
            ApiResponse<ResultWrapper<int>> apiResponse = await _foodService.DeleteFood(id);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }
    }
}

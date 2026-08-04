using AutoMapper;
using AutoWrapper.Wrappers;
using Common;
using Common.Model;
using MakanPergi.Entities.Views;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.Dtos.Food;
using MakanPergiWebApi.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MakanPergiWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IFoodService _foodService;
        public FoodController(IMapper mapper, IFoodService foodService)
        {
            _mapper = mapper;
            _foodService = foodService;
        }

        //[HttpGet]
        //public async Task<ApiResponse> GetAllFoods()
        //{
        //    try
        //    {
        //        IEnumerable<FoodViewModel> foodViewModels = await _foodService.GetAllFoodsAsync();

        //        return new ApiResponse("Foods fetched successfully", foodViewModels);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApiException(ex.Message);
        //    }
        //}

        [HttpPost("search")]
        public async Task<ApiResponse> GetAllFoods([FromBody] DatatablesRequest request)
        {
            try
            {
                //List<FoodViewModel> foodViewModel = (await _foodService.GetAllFoodsAsync()).ToList();
                List<VW_FoodFullView> foodFullViewModel = (await _foodService.GetAllFoodFullViewAsync()).ToList();

                if (!string.IsNullOrWhiteSpace(request?.Search?.Value) || !string.IsNullOrEmpty(request?.Search?.Value))
                {
                    foodFullViewModel = foodFullViewModel.Where(food =>
                                        food.Name.Contains(request.Search.Value, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                int foodViewModelCount = foodFullViewModel.Count;

                int sortColumnIndex = request.Order[0].Column;
                string sortDirection = request.Order[0].Dir ?? "asc";

                string? sortField = request.Columns[sortColumnIndex].Data;

                switch (sortField)
                {
                    case "id":
                        Console.WriteLine($"Sorting by Id ({sortDirection})");
                        foodFullViewModel = sortDirection == "asc"
                            ? foodFullViewModel.OrderBy(c => c.Id).ToList()
                            : foodFullViewModel.OrderByDescending(c => c.Id).ToList();
                        break;

                    case "name":
                        Console.WriteLine($"Sorting by Name ({sortDirection})");
                        foodFullViewModel = sortDirection == "asc"
                            ? foodFullViewModel.OrderBy(c => c.Name).ToList()
                            : foodFullViewModel.OrderByDescending(c => c.Name).ToList();
                        break;

                    case "cuisineName":
                        Console.WriteLine($"Sorting by Cuisine ({sortDirection})");
                        foodFullViewModel = sortDirection == "asc"
                            ? foodFullViewModel.OrderBy(c => c.CuisineName).ToList()
                            : foodFullViewModel.OrderByDescending(c => c.CuisineName).ToList();
                        break;

                    default:
                        Console.WriteLine($"Unknown field '{sortField}', defaulting to Name");
                        foodFullViewModel = sortDirection == "asc"
                            ? foodFullViewModel.OrderBy(c => c.Name).ToList()
                            : foodFullViewModel.OrderByDescending(c => c.Name).ToList();
                        break;
                }

                List<VW_FoodFullView> selectedFoodViewModel =
                                       foodFullViewModel.Skip(request.Start).Take(request.Length).ToList();

                Console.WriteLine($"Filtered foodViewModel - {selectedFoodViewModel.Count}");

                DataTablesResponse<VW_FoodFullView> response = new DataTablesResponse<VW_FoodFullView>
                {
                    Draw = request.Draw,
                    RecordsTotal = foodViewModelCount,
                    RecordsFiltered = foodViewModelCount,
                    Data = selectedFoodViewModel
                };

                Console.WriteLine(response.ToJsonString());

                return new ApiResponse("Foods fetched successfully", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Backend FoodController - Exception occurred");
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse> GetFoodById(string id)
        {
            try
            {
                FoodViewModel foodViewModel = await _foodService.GetFoodByIdAsync(Guid.Parse(id));

                return new ApiResponse($"Food with Id {foodViewModel.Id} fetched successfully", foodViewModel);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("foodEditViewModel/{id}")]
        public async Task<ApiResponse> GetFoodEditViewModelById(string id)
        {
            try
            {
                FoodEditViewModel foodEditViewModel = await _foodService.GetFoodEditViewModelByIdAsync(Guid.Parse(id));

                return new ApiResponse($"Food with Id {foodEditViewModel.Id} fetched successfully", foodEditViewModel);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResponse> AddNewFood([FromBody] CreateFoodDto createFoodDto)
        {
            try
            {
                FoodDataModel foodDataModel = _mapper.Map<FoodDataModel>(createFoodDto);
                await _foodService.AddNewFoodAsync(foodDataModel);

                return new ApiResponse("Food Added Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }

        }

        [HttpPut("update")]
        public async Task<ApiResponse> EditFoodAsync([FromBody] FoodEditViewModel foodEditViewModel)
        {
            try
            {
                List<TreeViewNodeModelDTO> shopNames = foodEditViewModel.ShopNames.ToList();
                List<TreeViewNodeModelDTO> filteredShopNames = shopNames.Where( shop => shop.Parent != "#")
                                                                        .ToList();
                foodEditViewModel.ShopNames = filteredShopNames.ToArray();

                FoodDataModel foodDataModel = _mapper.Map<FoodDataModel>(foodEditViewModel);
                await _foodService.UpdateFoodAsync(foodDataModel);

                return new ApiResponse("Food Updated Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ApiResponse> DeleteFood(Guid id)
        {
            try
            {
                await _foodService.DeleteFoodAsync(id);
                return new ApiResponse("Food Deleted Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }
    }
}

using Common.Model;
using MakanPergi.Models;
using MakanPergi.Models.Food;
using Refit;

namespace MakanPergi.Interfaces
{
    [Headers("Content-Type: application/json")]
    public interface IFoodService
    {
        [Post("/api/food/search")]
        Task<ApiResponse<ResultWrapper<DataTablesResponse<FoodFullViewModel>>>> GetAllFoods([Body] DatatablesRequest request);
        [Get("/api/food/{id}")]
        Task<ApiResponse<ResultWrapper<FoodEditViewModel>>> GetFoodById(Guid id);
        [Get("/api/food/foodEditViewModel/{id}")]
        Task<ApiResponse<ResultWrapper<FoodEditViewModel>>> GetFoodEditViewModelById(Guid id);
        [Post("/api/food")]
        Task<ApiResponse<ResultWrapper<int>>> AddNewFood(CreateFoodDto createFoodDto);
        [Put("/api/food/update")]
        Task<ApiResponse<ResultWrapper<int>>> EditFood(FoodEditViewModel foodEditViewModel);
        [Delete("/api/food/{id}")]
        Task<ApiResponse<ResultWrapper<int>>> DeleteFood(Guid id);
    }
}

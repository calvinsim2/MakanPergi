using MakanPergi.Entities.Views;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.ViewModels;

namespace MakanPergiWebApi.Interfaces.Services
{
    public interface IFoodService
    {
        Task<IEnumerable<FoodViewModel>> GetAllFoodsAsync();
        Task<IEnumerable<VW_FoodFullView>> GetAllFoodFullViewAsync();
        Task<FoodViewModel> GetFoodByIdAsync(Guid id);
        Task<FoodEditViewModel> GetFoodEditViewModelByIdAsync(Guid id);
        Task AddNewFoodAsync(FoodDataModel foodDataModel);
        Task UpdateFoodAsync(FoodDataModel foodDataModel);
        Task DeleteFoodAsync(Guid id);
    }
}

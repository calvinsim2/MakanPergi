using Common.Model;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.ViewModels;

namespace MakanPergiWebApi.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync();
        Task<IEnumerable<TreeViewNodeModel>> GetCategoriesTreeViewAsync();
        Task<IEnumerable<TreeViewNodeModel>> GetCategoriesTreeViewForSelectedFoodAsync(Guid id);
        Task<CategoryViewModel> GetCategoryByIdAsync(Guid id);
        Task AddNewCategoryAsync(CategoryDataModel categoryDataModel);
        Task UpdateCategoryAsync(CategoryDataModel categoryDataModel);
        Task DeleteCategoryAsync(Guid id);
    }
}

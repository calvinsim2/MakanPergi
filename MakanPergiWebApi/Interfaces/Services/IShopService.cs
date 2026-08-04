using Common.Model;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.ViewModels;

namespace MakanPergiWebApi.Interfaces.Services
{
    public interface IShopService
    {
        Task<IEnumerable<ShopViewModel>> GetAllShopsAsync();
        Task<DataTablesResponse<ShopIndexViewModel>> GetShopsDataTablesAsync(DatatablesRequest request);
        Task<IEnumerable<ShopViewModel>> GetAllRegionAsync();
        Task<IEnumerable<TreeViewNodeModel>> GetShopsTreeViewAsync();
        Task<IEnumerable<TreeViewNodeModel>> GetShopsTreeViewForSelectedFoodAsync(Guid id);
        Task<ShopViewModel> GetShopByIdAsync(Guid id);
        Task AddNewShopAsync(ShopDataModel shopDataModel);
        Task UpdateShopAsync(ShopDataModel shopDataModel);
        Task DeleteShopAsync(Guid id);
    }
}

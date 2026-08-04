using Common.Model;
using MakanPergi.Models;
using MakanPergi.Models.Shop;
using Refit;

namespace MakanPergi.Interfaces
{
    [Headers("Content-Type: application/json")]
    public interface IShopService
    {
        [Post("/api/shop/search")]
        Task<ApiResponse<ResultWrapper<DataTablesResponse<ShopIndexViewModel>>>> GetAllShops([Body] DatatablesRequest request);
        [Get("/api/shop/GetAllRegion")]
        Task<ApiResponse<ResultWrapper<List<ShopViewModel>>>> GetAllRegion();
        [Get("/api/shop/shopTreeView")]
        Task<ApiResponse<ResultWrapper<List<TreeViewNodeModel>>>> GetShopTreeView();
        [Get("/api/shop/shopTreeView/{id}")]
        Task<ApiResponse<ResultWrapper<List<TreeViewNodeModel>>>> GetShopTreeViewForSelectedFood(Guid id);
        [Get("/api/shop/{id}")]
        Task<ApiResponse<ResultWrapper<ShopViewModel>>> GetShopById(Guid id);
        [Post("/api/shop")]
        Task<ApiResponse<ResultWrapper<int>>> AddNewShop(CreateShopDto createShopDto);
        [Put("/api/shop/update")]
        Task<ApiResponse<ResultWrapper<int>>> EditShop(EditShopDto editShopDto);
        [Delete("/api/shop/{id}")]
        Task<ApiResponse<ResultWrapper<int>>> DeleteShop(Guid id);
    }
}

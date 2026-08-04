using AutoMapper;
using AutoWrapper.Wrappers;
using Common.Model;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.Dtos.Shop;
using MakanPergiWebApi.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MakanPergiWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController
    {
        private readonly IMapper _mapper;
        private readonly IShopService _shopService;
        public ShopController(IMapper mapper, IShopService shopService)
        {
            _mapper = mapper;
            _shopService = shopService;
        }

        [HttpGet]
        public async Task<ApiResponse> GetAllShopsViewModel()
        {
            try
            {
                IEnumerable<ShopViewModel> shopViewModels = await _shopService.GetAllShopsAsync();

                return new ApiResponse("Shops fetched successfully", shopViewModels);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpPost("search")]
        public async Task<ApiResponse> GetAllShops([FromBody] DatatablesRequest request)
        {
            try
            {
                DataTablesResponse<ShopIndexViewModel> shopIndexViewModel = 
                                    await _shopService.GetShopsDataTablesAsync(request);

                return new ApiResponse("Shop Index View Model fetched successfully", shopIndexViewModel);
            }
            catch (Exception ex) 
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("GetAllRegion")]
        public async Task<ApiResponse> GetAllRegion()
        {
            try
            {
                IEnumerable<ShopViewModel> shopViewModels = await _shopService.GetAllRegionAsync();

                return new ApiResponse("Regions fetched successfully", shopViewModels);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("shopTreeView")]
        public async Task<ApiResponse> GetShopTreeView()
        {
            try
            {
                List<TreeViewNodeModel> shopsNodeModel = (await _shopService.GetShopsTreeViewAsync()).ToList();

                return new ApiResponse("Shops Tree View fetched successfully", shopsNodeModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Backend GetShopTreeView - Exception occurred");
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("shopTreeView/{id}")]
        public async Task<ApiResponse> GetShopTreeViewForSelectedFood(Guid id)
        {
            List<TreeViewNodeModel> shopsNodeModel = 
                (await _shopService.GetShopsTreeViewForSelectedFoodAsync(id)).ToList();

            return new ApiResponse($"Shops Tree View for related Food Id - {id} fetched successfully", shopsNodeModel);
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse> GetShopById(string id)
        {
            try
            {
                ShopViewModel shopViewModel = await _shopService.GetShopByIdAsync(Guid.Parse(id));

                return new ApiResponse($"Shop with Id {shopViewModel.Id} fetched successfully", shopViewModel);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResponse> AddNewShop([FromBody] CreateShopDto createShopDto)
        {
            try
            {
                ShopDataModel shopDataModel = _mapper.Map<ShopDataModel>(createShopDto);
                await _shopService.AddNewShopAsync(shopDataModel);

                return new ApiResponse("Shop Added Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }

        }

        [HttpPut("update")]
        public async Task<ApiResponse> EditShop([FromBody] EditShopDto EditShopDto)
        {
            try
            {
                ShopDataModel shopDataModel = _mapper.Map<ShopDataModel>(EditShopDto);
                await _shopService.UpdateShopAsync(shopDataModel);

                return new ApiResponse("Shop Updated Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ApiResponse> RemoveShopAsync(Guid id)
        {
            try
            {
                await _shopService.DeleteShopAsync(id);
                return new ApiResponse("Shop Deleted Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }
    }
}

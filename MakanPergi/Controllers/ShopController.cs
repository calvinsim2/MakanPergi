using AutoMapper;
using Common;
using Common.Model;
using MakanPergi.Common;
using MakanPergi.Interfaces;
using MakanPergi.Models;
using MakanPergi.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace MakanPergi.Controllers
{
    [Route("[controller]")]
    public class ShopController : Controller
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IMapper _mapper;
        private readonly IShopService _shopService;

        public ShopController(ILogger<ShopController> logger, IMapper mapper, IShopService shopService)
        {
            _logger = logger;
            _mapper = mapper;
            _shopService = shopService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("LoadShopsData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadShopsData(DatatablesRequest request)
        {
            ApiResponse<ResultWrapper<DataTablesResponse<ShopIndexViewModel>>> apiResponse =
                                                                            await _shopService.GetAllShops(request);
            DataTablesResponse<ShopIndexViewModel>? shopDataTables = null;
            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                Console.WriteLine($"Results found!");
                Console.WriteLine($"{apiResponse.Content.ToJsonString()}");
                shopDataTables = apiResponse?.Content?.Result;
            }

            Console.WriteLine(shopDataTables.ToJsonString());
            // DataTables expects a specific JSON shape when using server-side processing
            return Json(shopDataTables);

        }

        [HttpGet("GetCreateShopModal")]
        public async Task<IActionResult> GetCreateShopModal()
        {
            CreateShopDto createShopDto = new CreateShopDto();

            var cuisineServiceResponse = await _shopService.GetAllRegion();

            ViewBag.AllShop = cuisineServiceResponse.IsSuccessStatusCode &&
                                 cuisineServiceResponse.Content.Result is not null ?
                                 cuisineServiceResponse.Content.Result : new List<ShopViewModel>();

            return PartialView("_CreateShopModal", createShopDto);
        }

        [HttpPost("CreateShop")]
        [Route("CreateShop", Name = "shop.create")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateShop(CreateShopDto createShopDto)
        {
            Console.WriteLine(createShopDto.ToJsonString());
            ApiResponse<ResultWrapper<int>> apiResponse = await _shopService.AddNewShop(createShopDto);

            return new ApiResponseActionResult<int>(apiResponse, true);

        }

        [HttpPost("UpdateShop")]
        [Route("UpdateShop", Name = "shop.update")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditShop(EditShopDto editShopDtoRequest)
        {
            Console.WriteLine($"EditShopDto (Before calling backend): {editShopDtoRequest.ToJsonString()}");
            ApiResponse<ResultWrapper<int>> apiResponse = await _shopService.EditShop(editShopDtoRequest);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }

        [HttpGet("GetEditShopModal/{id}")]
        public async Task<IActionResult> GetEditShopModal(Guid id)
        {
            ApiResponse<ResultWrapper<ShopViewModel>> apiResponse = await _shopService.GetShopById(id);
            if (apiResponse == null)
            {
                return StatusCode(500, "Unexpected null response from service.");
            }

            if (!apiResponse.IsSuccessStatusCode || apiResponse.Content is null)
            {
                // Optionally log apiResponse.Message
                return StatusCode((int)apiResponse.StatusCode);
            }

            var cuisineServiceResponse = await _shopService.GetAllRegion();

            ViewBag.AllShop = cuisineServiceResponse.IsSuccessStatusCode &&
                                 cuisineServiceResponse.Content.Result is not null ?
                                 cuisineServiceResponse.Content.Result : new List<ShopViewModel>();

            EditShopDto editShopDto = new EditShopDto
            {
                Id = apiResponse.Content.Result.Id,
                Name = apiResponse.Content.Result.Name,
                Image = apiResponse.Content.Result.Image,
                Address = apiResponse.Content.Result.Address,
                RegionId = apiResponse.Content.Result.RegionId
            };

            Console.WriteLine($"EditShopDto: {editShopDto.ToJsonString()}");
            return PartialView("_EditShopModal", editShopDto);

        }

        [HttpDelete("DeleteShop/{id}")]
        [Route("DeleteShop", Name = "shop.delete")]
        public async Task<IActionResult> DeleteShop(Guid id)
        {
            ApiResponse<ResultWrapper<int>> apiResponse = await _shopService.DeleteShop(id);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }
    }
}

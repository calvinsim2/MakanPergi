using AutoMapper;
using AutoWrapper.Wrappers;
using Common;
using Common.Model;
using Common.Model.Excel;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.Dtos.Cuisine;
using MakanPergiWebApi.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MakanPergiWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuisineController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICuisineService _cuisineService;

        public CuisineController(IMapper mapper, ICuisineService cuisineService)
        {
            _mapper = mapper;
            _cuisineService = cuisineService;
        }

        [HttpPost("search")]
        public async Task<ApiResponse> GetAllCuisines([FromBody] DatatablesRequest request)
        {
            try
            {

                IEnumerable<CuisineDataModel> cuisineDataModel = await _cuisineService.GetAllCuisineAsync();
                List<CuisineViewModel> cuisineViewModels = _mapper.Map<List<CuisineViewModel>>(cuisineDataModel);

                if (!string.IsNullOrWhiteSpace(request?.Search?.Value) || !string.IsNullOrEmpty(request?.Search?.Value))
                {
                    cuisineViewModels = cuisineViewModels.Where(cuisine =>
                                        cuisine.Name.Contains(request.Search.Value, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                int cuisineViewModelsCount = cuisineViewModels.Count;

                // Order array, is given like this [ {Column: "" , Dir: ""}]
                // in this example, it's the basic and simplest sorting, where Order array only have 1.
                // else, we need to use foreach
                int sortColumnIndex = request.Order[0].Column;
                string sortDirection = request.Order[0].Dir ?? "asc";

                string? sortField = request.Columns[sortColumnIndex].Data;

                switch (sortField)
                {
                    case "id":
                        Console.WriteLine($"Sorting by Id ({sortDirection})");
                        cuisineViewModels = sortDirection == "asc"
                            ? cuisineViewModels.OrderBy(c => c.Id).ToList()
                            : cuisineViewModels.OrderByDescending(c => c.Id).ToList();
                        break;

                    case "name":
                        Console.WriteLine($"Sorting by Name ({sortDirection})");
                        cuisineViewModels = sortDirection == "asc"
                            ? cuisineViewModels.OrderBy(c => c.Name).ToList()
                            : cuisineViewModels.OrderByDescending(c => c.Name).ToList();
                        break;

                    default:
                        Console.WriteLine($"Unknown field '{sortField}', defaulting to Name");
                        cuisineViewModels = sortDirection == "asc"
                            ? cuisineViewModels.OrderBy(c => c.Name).ToList()
                            : cuisineViewModels.OrderByDescending(c => c.Name).ToList();
                        break;
                }

                List<CuisineViewModel> selectedCuisineViewModels =
                                       cuisineViewModels.Skip(request.Start).Take(request.Length).ToList();

                Console.WriteLine($"Filtered cuisineViewModels - {selectedCuisineViewModels.Count}");

                DataTablesResponse<CuisineViewModel> response = new DataTablesResponse<CuisineViewModel>
                {
                    Draw = request.Draw,
                    RecordsTotal = cuisineViewModelsCount,
                    RecordsFiltered = cuisineViewModelsCount,
                    Data = selectedCuisineViewModels
                };

                Console.WriteLine(response.ToJsonString());

                return new ApiResponse("Cuisines fetched successfully", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Backend CuisineController - Exception occured");
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ApiResponse> GetAllCuisineViewModel()
        {
            try
            {
                List<CuisineDataModel> cuisineDataModels = (await _cuisineService.GetAllCuisineAsync()).ToList();
                List<CuisineViewModel> cuisineViewModels = _mapper.Map<List<CuisineViewModel>>(cuisineDataModels);

                return new ApiResponse("CuisineViewModels fetched successfully", cuisineViewModels);
            }
            catch (Exception ex) 
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ApiResponse> GetCuisineById(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                throw new ApiException("Id is empty", StatusCodes.Status400BadRequest);
            }

            CuisineDataModel cuisineDataModel = await _cuisineService.GetCuisineByIdProcessAsync(id);

            CuisineViewModel cuisineViewModel = _mapper.Map<CuisineViewModel>(cuisineDataModel);

            return new ApiResponse("Cuisine fetched successfully", cuisineViewModel, StatusCodes.Status200OK);
        }

        [HttpPost]
        public async Task<ApiResponse> CreateCuisine([FromBody] CreateCuisineDto createCuisineDto)
        {
            try
            {
                CuisineDataModel createCuisineDataModel = _mapper.Map<CuisineDataModel>(createCuisineDto);
                await _cuisineService.CreateCuisineProcessAsync(createCuisineDataModel);

                return new ApiResponse("Cuisine Added Successfully", StatusCodes.Status201Created, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }

        }

        [HttpPut("update")]
        public async Task<ApiResponse> EditCuisineAsync([FromBody] EditCuisineDto editCuisineDto)
        {
            try
            {
                CuisineDataModel editCuisineDataModel = _mapper.Map<CuisineDataModel>(editCuisineDto);
                await _cuisineService.UpdateCuisineProcessAsync(editCuisineDataModel);

                return new ApiResponse("Cuisine Updated Successfully", StatusCodes.Status200OK, StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(EditCuisineAsync)} - Error message: {ex.Message}");
                // throwing an exception, in order for the message to be read at front end is possible via
                // the assistance from AutoWrappers. It removes the additional stack trace messages.
                throw new ApiException(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ApiResponse> RemoveCuisineAsync(Guid id)
        {
            try
            {
                await _cuisineService.DeleteCuisineProcessAsync(id);
                return new ApiResponse("Cuisine Deleted Successfully");
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        [HttpPost("ExcelUpload")]
        public async Task<ApiResponse> ExcelUpload(List<UploadCuisineExcelData> uploadCuisineExcelDatas)
        {
            return await _cuisineService.CuisineExcelUploadProcessAsync(uploadCuisineExcelDatas);

        }
    }
}

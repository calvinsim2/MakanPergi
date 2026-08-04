using AutoMapper;
using Common;
using Common.Model;
using Common.Model.Excel;
using MakanPergi.Common;
using MakanPergi.Common.Helpers;
using MakanPergi.Interfaces;
using MakanPergi.Models;
using MakanPergi.Models.Cuisine;
using MakanPergi.Models.Excel;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Refit;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MakanPergi.Controllers
{
    [Route("[controller]")]
    public class CuisineController : Controller
    {
        private readonly ILogger<CuisineController> _logger;
        private readonly IMapper _mapper;
        private readonly ICuisineService _cuisineService;

        public CuisineController(ILogger<CuisineController> logger, IMapper mapper, ICuisineService cuisineService)
        {
            _logger = logger;
            _mapper = mapper;
            _cuisineService = cuisineService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            return View();
        }

        [HttpPost("LoadCuisineData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadCuisineData(DatatablesRequest request)
        {
            Console.WriteLine($"Calling GetAllCuisines from backend");
            Console.WriteLine($"{JsonSerializer.Serialize(request)}");
            ApiResponse<ResultWrapper<DataTablesResponse<CuisineViewModel>>> apiResponse = await _cuisineService.GetAllCuisines(request);
            DataTablesResponse<CuisineViewModel>? cuisineDataTables = null;
            if (apiResponse is not null && apiResponse.IsSuccessStatusCode && apiResponse.Content is not null)
            {
                Console.WriteLine($"Results found!");
                Console.WriteLine($"{apiResponse.Content.ToJsonString()}");
                cuisineDataTables = apiResponse?.Content?.Result;
            }

            Console.WriteLine(cuisineDataTables.ToJsonString());
            // DataTables expects a specific JSON shape when using server-side processing
            return Json(cuisineDataTables);

        }

        [HttpPost("CreateCuisine")]
        [Route("CreateCuisine", Name = "cuisine.create")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateCuisine(CreateCuisineDto createCuisineDtoRequest)
        {
            ApiResponse<ResultWrapper<int>> apiResponse = await _cuisineService.CreateCuisine(createCuisineDtoRequest);

            return new ApiResponseActionResult<int>(apiResponse, true);

        }

        [HttpGet("GetCreateCuisineModal")]
        public IActionResult GetCreateCuisineModal()
        {
            CreateCuisineDto createCuisineDto = new CreateCuisineDto();

            return PartialView("_CreateCuisineModal", createCuisineDto);
        }

        [HttpPost("UpdateCuisine")]
        [Route("UpdateCuisine", Name = "cuisine.update")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditCuisine(EditCuisineDto editCuisineDtoRequest)
        {
            //if (!ModelState.IsValid)
            //{
            //    return StatusCode(StatusCodes.Status400BadRequest);
            //}

            Console.WriteLine($"EditCuisineDto (Before calling backend): {editCuisineDtoRequest.ToJsonString()}");
            ApiResponse<ResultWrapper<int>> apiResponse = await _cuisineService.EditCuisine(editCuisineDtoRequest);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }

        [HttpGet("GetEditCuisineModal/{id}")]
        public async Task<IActionResult> GetEditCuisineModal(Guid id)
        {
            ApiResponse<ResultWrapper<CuisineViewModel>> apiResponse = await _cuisineService.GetCuisineById(id);
            if (apiResponse == null)
            {
                return StatusCode(500, "Unexpected null response from service.");
            }

            if (!apiResponse.IsSuccessStatusCode || apiResponse.Content is null)
            {
                // Optionally log apiResponse.Message
                return StatusCode((int)apiResponse.StatusCode);
            }

            EditCuisineDto editCuisineDto = new EditCuisineDto
            {
                Id = apiResponse.Content.Result.Id,
                Name = apiResponse.Content.Result.Name
            };

            Console.WriteLine($"EditCuisineDto: {editCuisineDto.ToJsonString()}");
            return PartialView("_EditCuisineModal", editCuisineDto);

        }

        [HttpDelete("DeleteCuisine/{id}")]
        [Route("DeleteCuisine", Name = "cuisine.delete")]
        public async Task<IActionResult> DeleteCuisine(Guid id)
        {
            ApiResponse<ResultWrapper<int>> apiResponse = await _cuisineService.DeleteCuisine(id);

            return new ApiResponseActionResult<int>(apiResponse, true);
        }

        [HttpGet("CuisineBulkUpload")]
        [Route("CuisineBulkUpload", Name = "cuisine.bulkUpload")]
        public IActionResult ShowCuisineBulkUploadPage()
        {
            Console.WriteLine($"{nameof(ShowCuisineBulkUploadPage)} triggered");
            return View("_CuisineBulkUpload");
        }

        [HttpPost("ExcelUpload")]
        public async Task<IActionResult> ExcelUpload([FromForm] IFormFile file)
        {
            Console.WriteLine($"{nameof(ExcelUpload)} triggered");
            List<UploadCuisineExcelData> uploadCuisineExcelData = ProcessCuisineExcelData(file);

            if (uploadCuisineExcelData.Count == 1 && uploadCuisineExcelData[0].ErrorList.Any())
            {
                ResultWrapper<List<UploadExcelResult>> uploadExcelResultWrapper = new ResultWrapper<List<UploadExcelResult>>();
                uploadExcelResultWrapper.Message = "0 Cuisine(s) has been uploaded.";
                uploadExcelResultWrapper.Result = uploadCuisineExcelData[0].ErrorList;

                var noDataResponse = new
                {
                    TotalCount = uploadCuisineExcelData[0].TotalCount,
                    ResultList = uploadExcelResultWrapper.Result,
                    Message = uploadExcelResultWrapper.Message

                };

                return Ok(noDataResponse);
            }

            var excelUploadResponse = await _cuisineService.ExcelUpload(uploadCuisineExcelData);

            if (!excelUploadResponse.IsSuccessStatusCode || excelUploadResponse.Content is null)
            {
                return Ok(excelUploadResponse.Error);
            }

            List<UploadExcelResult> excelUpload = excelUploadResponse.Content.Result!;
            var response = new { TotalCount = uploadCuisineExcelData[0].TotalCount, ResultList = excelUpload };
            return Ok(response);
        }

        private List<UploadCuisineExcelData>? ProcessCuisineExcelData([FromForm] IFormFile file)
        {
            if (file is null)
            {
                return null;
            }

            List<UploadCuisineExcelData> uploadCuisineExcelDataList = new List<UploadCuisineExcelData>();
            List<UploadExcelResult> results = new List<UploadExcelResult>();

            int totalCount = 0;

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var workbook = new XSSFWorkbook(stream);
                    ISheet sheet = workbook.GetSheetAt(0);

                    for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++) 
                    { 
                        IRow? currentRow = sheet.GetRow(rowIndex);

                        if (string.IsNullOrEmpty(currentRow.GetCellValue(0)))
                        {
                            continue;
                        }

                        List<UploadExcelResult> validationResults = ValidateCuisineRow(currentRow, rowIndex);
                        results.AddRange(validationResults);

                        if (validationResults.All( result => result.IsValid))
                        {
                            Console.WriteLine($"All cells at row: {rowIndex} is valid!");
                            uploadCuisineExcelDataList.Add(new UploadCuisineExcelData
                            {
                                EntityName = currentRow.GetCellValue(0),
                            });
                        }
                    }

                    totalCount = uploadCuisineExcelDataList.Count;
                    workbook.Close();

                    if (results.Any() && !uploadCuisineExcelDataList.Any())
                    {
                        uploadCuisineExcelDataList.Add(new UploadCuisineExcelData
                        {
                            ErrorList = new List<UploadExcelResult>()
                        });
                    }

                    uploadCuisineExcelDataList[0].TotalCount = totalCount;
                    uploadCuisineExcelDataList[0].ErrorList = results.Where(result => !result.IsValid).ToList();

                    return uploadCuisineExcelDataList;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                results.Add(new UploadExcelResult
                {
                    EntityName = "Unknown",
                    Message = $"Error during processing: {ex.Message}",
                    IsValid = false
                });

                return null;
            }

            
        }

        private List<UploadExcelResult> ValidateCuisineRow(IRow currentRow, int rowIndex)
        {
            List<UploadExcelResult> validationResults = new List<UploadExcelResult>();

            // GetSheetAt(<sheetIndex>) -> retrieves the WORKSHEET at excel, the first page, has an index of 0.
            // GetRow(<rowIndex>) -> retrieves the entire row in the worksheet.

            Console.WriteLine("Name:" + currentRow.GetCellValue(0));
            string? entityName = ExcelGeneralMethods.GetCellStringValue(currentRow, 0);
            if (string.IsNullOrEmpty(entityName))
            {
                ExcelGeneralMethods.AddError(validationResults, currentRow, 0, rowIndex, "Name cannot be empty");
            }
            else if (!Regex.IsMatch(entityName, @"^[A-Za-z]+$"))
            {
                ExcelGeneralMethods.AddError(
                    validationResults,
                    currentRow,
                    0,
                    rowIndex,
                    "Name may only contain letters A–Z"
                );
            }
            else
            {
                ExcelGeneralMethods.AddSuccess(validationResults, 0, currentRow);
            }

            return validationResults;
        }

        [Post("GetCuisineTemplate")]
        public ActionResult GetCuisineTemplate(CancellationToken cancellationToken = default)
        {
            var excelFile = GetCuisineTemplateFile();
            if (excelFile != null)
            {
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CuisineTemplate.xlsx");
            }
            return null;
        }

        private ExcelFileStream GetCuisineTemplateFile(CancellationToken cancellationToken = default)
        {
            string path = Environment.CurrentDirectory + "\\wwwroot\\templates\\CuisineTemplate.xlsx";
            var workbook = new XSSFWorkbook(path);
            //Location
            //var responseLocation = await _visitorService.GetLocationsTreeView(cancellationToken);
            //if (responseLocation != null && responseLocation.Content != null && responseLocation.Content.Result != null)
            //{
            //    var excelLocation = responseLocation.Content.Result.Select(x => x.Text).ToArray();
            //    workbook = ExcelEx.SetConstraint(workbook, 1, excelLocation, "Location", 8);
            //}


            var file = new ExcelFileStream();
            workbook.Write(file);
            file.Flush();
            file.Seek(0, SeekOrigin.Begin);
            file.AllowClose = true;

            return file;
        }
    }
}

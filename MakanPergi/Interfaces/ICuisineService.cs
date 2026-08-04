using Common.Model;
using Common.Model.Excel;
using MakanPergi.Models;
using MakanPergi.Models.Cuisine;
using Refit;

namespace MakanPergi.Interfaces
{
    [Headers("Content-Type: application/json")]
    public interface ICuisineService
    {
        [Post("/api/cuisine/search")]
        Task<ApiResponse<ResultWrapper<DataTablesResponse<CuisineViewModel>>>> GetAllCuisines([Body] DatatablesRequest request);
        [Get("/api/cuisine")]
        Task<ApiResponse<ResultWrapper<List<CuisineViewModel>>>> GetAllCuisineViewModel();
        [Post("/api/cuisine")]
        Task<ApiResponse<ResultWrapper<int>>> CreateCuisine(CreateCuisineDto createCuisineDto);
        [Get("/api/cuisine/{id}")]
        Task<ApiResponse<ResultWrapper<CuisineViewModel>>> GetCuisineById(Guid id);
        [Put("/api/cuisine/update")]
        Task<ApiResponse<ResultWrapper<int>>> EditCuisine(EditCuisineDto editCuisineDto);
        [Delete("/api/cuisine/{id}")]
        Task<ApiResponse<ResultWrapper<int>>> DeleteCuisine(Guid id);
        [Post("/api/cuisine/ExcelUpload")]
        Task<ApiResponse<ResultWrapper<List<UploadExcelResult>>>> ExcelUpload(List<UploadCuisineExcelData> uploadCuisineExcelDatas);
    }
}

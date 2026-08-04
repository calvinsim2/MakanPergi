using AutoWrapper.Wrappers;
using Common.Model.Excel;
using MakanPergiWebApi.Models.DataModels;

namespace MakanPergiWebApi.Interfaces.Services
{
    public interface ICuisineService
    {
        Task<IEnumerable<CuisineDataModel>> GetAllCuisineAsync();
        Task<CuisineDataModel> GetCuisineByIdProcessAsync(Guid id);
        Task CreateCuisineProcessAsync(CuisineDataModel cuisineDataModel);
        Task UpdateCuisineProcessAsync(CuisineDataModel cuisineDataModel);
        Task DeleteCuisineProcessAsync(Guid id);
        Task<ApiResponse> CuisineExcelUploadProcessAsync(List<UploadCuisineExcelData> uploadCuisineExcelDatas);
    }
}

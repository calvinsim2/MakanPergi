using Common.Model;
using MakanPergi.Models;
using MakanPergi.Models.Category;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace MakanPergi.Interfaces
{
    [Headers("Content-Type: application/json")]
    public interface ICategoryService
    {
        [Post("/api/category/search")]
        Task<ApiResponse<ResultWrapper<DataTablesResponse<CategoryViewModel>>>> GetAllCategories([Body] DatatablesRequest request);
        [Get("/api/category/categoryTreeView")]
        Task<ApiResponse<ResultWrapper<List<TreeViewNodeModel>>>> GetCategoryTreeView();
        [Get("/api/category/categoryTreeView/{id}")]
        Task<ApiResponse<ResultWrapper<List<TreeViewNodeModel>>>> GetCategoryTreeViewForSelectedFood(Guid id);
        [Post("/api/category")]
        Task<ApiResponse<ResultWrapper<int>>> AddNewCategoryAsync([FromBody] CreateCategoryDto createCategoryDto);
        [Get("/api/category/{id}")]
        Task<ApiResponse<ResultWrapper<CategoryViewModel>>> GetCategoryById(Guid id);
        [Put("/api/category/update")]
        Task<ApiResponse<ResultWrapper<int>>> EditCategory(EditCategoryDto editCategoryDto);
        [Delete("/api/category/{id}")]
        Task<ApiResponse<ResultWrapper<int>>> DeleteCategory(Guid id);
    }
}

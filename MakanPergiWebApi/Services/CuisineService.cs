using AutoMapper;
using AutoWrapper.Wrappers;
using Common.Model.Excel;
using MakanPergi.Entities;
using MakanPergiWebApi.Controllers;
using MakanPergiWebApi.Interfaces.Repository;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MakanPergiWebApi.Services
{
    public class CuisineService : BaseService, ICuisineService
    {
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPortalService _portalService;
        private readonly ILogger<CuisineService> _logger;

        public CuisineService(IUnitOfWork unitofWork, 
                              IServiceProvider serviceProvider,
                              IPortalService portalService,
                              IMapper mapper,
                              ILogger<CuisineService> logger) : base(unitofWork)
        {
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _portalService = portalService;
            _logger = logger;
        }

        public async Task<IEnumerable<CuisineDataModel>> GetAllCuisineAsync()
        {
            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();

            List<Cuisine> cuisines = await cuisineRepository.TableNoTracking.ToListAsync();

            List<CuisineDataModel> cuisinesDataModel = _mapper.Map<List<CuisineDataModel>>(cuisines);

            return cuisinesDataModel;
        }

        public async Task<CuisineDataModel> GetCuisineByIdProcessAsync(Guid id)
        {
            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();

            Cuisine? cuisine = await cuisineRepository.TableNoTracking.Where(cuisine => cuisine.Id == id).FirstOrDefaultAsync();

            if (cuisine is null)
            {
                throw new ApiException("No cuisine found", StatusCodes.Status404NotFound);
            }

            CuisineDataModel cuisineDataModel = _mapper.Map<CuisineDataModel>(cuisine);

            return cuisineDataModel;
        }

        public async Task CreateCuisineProcessAsync(CuisineDataModel cuisineDataModel)
        {
            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();

            Cuisine cuisine = _mapper.Map<Cuisine>(cuisineDataModel);

            await cuisineRepository.AddAsync(cuisine);
            await WorkContext.CompleteAsync();

        }

        public async Task UpdateCuisineProcessAsync(CuisineDataModel cuisineDataModel)
        {
            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();

            Cuisine? cuisine = await cuisineRepository.Table.Where(c => c.Id == cuisineDataModel.Id).FirstOrDefaultAsync(); 

            if (cuisine is null)
            {
                throw new ApiException("No cuisine found", StatusCodes.Status404NotFound);
            }

            cuisine.Name = cuisineDataModel.Name;

            cuisineRepository.Update(cuisine);
            await WorkContext.CompleteAsync();
        }

        public async Task DeleteCuisineProcessAsync(Guid id)
        {
            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();

            Cuisine? cuisine = await cuisineRepository.Table.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (cuisine is null)
            {
                throw new ApiException("No cuisine found", StatusCodes.Status404NotFound);
            }

            cuisineRepository.Remove(cuisine);
            await WorkContext.CompleteAsync();
        }

        public async Task<ApiResponse> CuisineExcelUploadProcessAsync(List<UploadCuisineExcelData> uploadCuisineExcelDatas)
        {
            List<UploadExcelResult> uploadExcelResult = uploadCuisineExcelDatas[0]?.ErrorList ?? new List<UploadExcelResult>();

            if (uploadCuisineExcelDatas is null || !uploadCuisineExcelDatas.Any())
            {
                return new ApiResponse($"No cuisine(s) have been uploaded. Excel sheet rows are empty.", uploadExcelResult, StatusCodes.Status200OK);
            }

            int successCount = 0, recordProcessCount = 0;
            string webSocketActionName = $"_UploadCuisine_Progress";

            IRepository<Cuisine>? cuisineRepository = WorkContext.Repository<Cuisine>();
            _logger.LogInformation("Before loading existing cuisines");
            var sw = Stopwatch.StartNew();

            List<Cuisine> existingCuisines = await cuisineRepository.TableNoTracking.ToListAsync();
            sw.Stop();
            _logger.LogInformation("Loaded existing cuisines in {ElapsedMs} ms. Count = {Count}", sw.ElapsedMilliseconds, existingCuisines.Count);

            for (int row = 0; row < uploadCuisineExcelDatas.Count; row++)
            {
                recordProcessCount++;

                using IServiceScope? scope = _serviceProvider.CreateScope();
                IPortalService? scopedPortalService = scope.ServiceProvider.GetRequiredService<IPortalService>();

                _logger.LogInformation("Before SendMessage for row {Row}", row);

                await _portalService.SendMessage(new Common.Model.WebSocketMsg 
                { 
                    Action = webSocketActionName,
                    JsonMessageBody = JsonConvert.SerializeObject(new UploadProgressModel() 
                    {
                        Current = recordProcessCount,
                        Total = uploadCuisineExcelDatas.Count,
                        IsComplete = false
                    }),
                });

                _logger.LogInformation("After SendMessage for row {Row}", row);
                UploadExcelResult currentUploadExcelResult = new UploadExcelResult();


                IUnitOfWork? scopedDbContext = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IRepository<Cuisine> scopedCuisineRepository = scopedDbContext.Repository<Cuisine>();

                try
                {
                    string entityName = uploadCuisineExcelDatas[row].EntityName.Trim();
                    if (existingCuisines.Any(existingCuisine => existingCuisine.Name == entityName))
                    {
                        Console.WriteLine($"{nameof(CuisineExcelUploadProcessAsync)} - {entityName} exists in db.");
                        currentUploadExcelResult.EntityName = uploadCuisineExcelDatas[row].EntityName;
                        currentUploadExcelResult.Message = $"{entityName} exists in db.";
                        currentUploadExcelResult.IsValid = false;
                        uploadExcelResult!.Add(currentUploadExcelResult);

                        continue;
                    }

                    Cuisine newCuisine = new Cuisine
                    {
                        Name = entityName
                    };

                    _logger.LogInformation("Before DB save for row {Row}", row);
                    await scopedCuisineRepository.AddAsync(newCuisine);
                    await scopedDbContext.CompleteAsync();
                    _logger.LogInformation("After DB save for row {Row}", row);

                    currentUploadExcelResult.EntityName = uploadCuisineExcelDatas[row].EntityName;
                    currentUploadExcelResult.IsValid = true;
                    uploadExcelResult!.Add(currentUploadExcelResult);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error at {row}: {uploadCuisineExcelDatas[row].EntityName}");
                    currentUploadExcelResult.EntityName = uploadCuisineExcelDatas[row].EntityName;
                    currentUploadExcelResult.Message = $"Error at {row}: {ex.Message}";
                    currentUploadExcelResult.IsValid = false;

                }

            }

            return new ApiResponse($"{successCount} cuisine(s) successfully uploaded.", uploadExcelResult, StatusCodes.Status200OK);

        }
    }
}

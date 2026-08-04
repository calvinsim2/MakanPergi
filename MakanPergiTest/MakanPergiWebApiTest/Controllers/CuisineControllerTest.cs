using AutoMapper;
using AutoWrapper.Wrappers;
using Common.Model;
using Common.Model.Excel;
using FakeItEasy;
using MakanPergiWebApi.Controllers;
using MakanPergiWebApi.Helpers;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.Dtos.Cuisine;
using MakanPergiWebApi.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace MakanPergiTest.MakanPergiWebApiTest.Controllers
{
    public class CuisineControllerTest
    {
        // Real mapper, not a fake - AutoMapper's configuration is pure, so using the
        // actual GlobalProfile also covers the CuisineDataModel -> CuisineViewModel map.
        // Static because building a MapperConfiguration is expensive and it is immutable.
        private static readonly IMapper _mapper =
            new MapperConfiguration(cfg => cfg.AddProfile<GlobalProfile>(), NullLoggerFactory.Instance)
                .CreateMapper();

        private readonly ICuisineService _cuisineServiceMock = A.Fake<ICuisineService>();

        private readonly CuisineController _cuisineController;

        public CuisineControllerTest()
        {
            _cuisineController = new CuisineController(_mapper, _cuisineServiceMock);
        }

        // Fixed Ids so that ordering by Id is deterministic. Guid.CompareTo compares the
        // leading 32-bit field first, so these sort 1 < 2 < 3.
        private static readonly Guid _westernIdMock = new Guid("00000001-0000-0000-0000-000000000000");
        private static readonly Guid _chineseIdMock = new Guid("00000002-0000-0000-0000-000000000000");
        private static readonly Guid _malayIdMock = new Guid("00000003-0000-0000-0000-000000000000");

        /// <summary>
        /// Names are deliberately out of step with the Ids so that sorting by "id" and
        /// sorting by "name" produce different orders - otherwise the switch in
        /// GetAllCuisines could take the wrong branch and still pass.
        /// </summary>
        private static List<CuisineDataModel> BuildCuisineDataModelsMock()
        {
            return new List<CuisineDataModel>
            {
                new CuisineDataModel { Id = _westernIdMock, Name = "Western" },
                new CuisineDataModel { Id = _chineseIdMock, Name = "Chinese" },
                new CuisineDataModel { Id = _malayIdMock, Name = "Malay" }
            };
        }

        private static DatatablesRequest BuildDatatablesRequestMock(string? searchValue = null,
                                                                   string sortField = "name",
                                                                   string? sortDirection = "asc",
                                                                   int start = 0,
                                                                   int length = 10,
                                                                   int draw = 1)
        {
            return new DatatablesRequest
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = new DatatablesSearch { Value = searchValue },
                Columns = new List<DatatablesColumn>
                {
                    new DatatablesColumn { Data = sortField }
                },
                Order = new List<DatatablesOrder>
                {
                    // Dir is declared non-nullable but the controller still guards it with
                    // "?? asc", so null is forced through here to cover that branch.
                    new DatatablesOrder { Column = 0, Dir = sortDirection! }
                }
            };
        }

        #region GetAllCuisines

        [Fact]
        public async Task GetAllCuisines_Positive_CuisinesExist_ShouldReturnNonEmptyDataTablesResponse()
        {
            List<CuisineDataModel> cuisineDataModelsMock = BuildCuisineDataModelsMock();
            DatatablesRequest datatablesRequestMock = BuildDatatablesRequestMock(draw: 7);

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisines(datatablesRequestMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            DataTablesResponse<CuisineViewModel> dataTablesResponse =
                Assert.IsAssignableFrom<DataTablesResponse<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.NotEmpty(dataTablesResponse.Data);
            Assert.Equal(3, dataTablesResponse.Data.Count);
            Assert.Equal(7, dataTablesResponse.Draw);
            Assert.Equal(3, dataTablesResponse.RecordsTotal);
            Assert.Equal(3, dataTablesResponse.RecordsFiltered);
        }

        [Fact]
        public async Task GetAllCuisines_Negative_NoCuisineExists_ShouldReturnEmptyDataTablesResponse()
        {
            List<CuisineDataModel> cuisineDataModelsMock = new List<CuisineDataModel>();
            DatatablesRequest datatablesRequestMock = BuildDatatablesRequestMock();

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisines(datatablesRequestMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            DataTablesResponse<CuisineViewModel> dataTablesResponse =
                Assert.IsAssignableFrom<DataTablesResponse<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Empty(dataTablesResponse.Data);
            Assert.Equal(0, dataTablesResponse.RecordsTotal);
        }

        [Fact]
        public async Task GetAllCuisines_Positive_SearchValueMatchesCuisine_ShouldReturnOnlyMatchingCuisine()
        {
            List<CuisineDataModel> cuisineDataModelsMock = BuildCuisineDataModelsMock();
            // Lower case on purpose - the filter is OrdinalIgnoreCase.
            DatatablesRequest datatablesRequestMock = BuildDatatablesRequestMock(searchValue: "chi");

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisines(datatablesRequestMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            DataTablesResponse<CuisineViewModel> dataTablesResponse =
                Assert.IsAssignableFrom<DataTablesResponse<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            CuisineViewModel cuisineViewModel = Assert.Single(dataTablesResponse.Data);
            Assert.Equal("Chinese", cuisineViewModel.Name);
            // The counts are taken after filtering, so they narrow to the match too.
            Assert.Equal(1, dataTablesResponse.RecordsTotal);
            Assert.Equal(1, dataTablesResponse.RecordsFiltered);
        }

        [Fact]
        public async Task GetAllCuisines_Negative_SearchValueMatchesNoCuisine_ShouldReturnEmptyDataTablesResponse()
        {
            List<CuisineDataModel> cuisineDataModelsMock = BuildCuisineDataModelsMock();
            DatatablesRequest datatablesRequestMock = BuildDatatablesRequestMock(searchValue: "NoSuchCuisine");

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisines(datatablesRequestMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            DataTablesResponse<CuisineViewModel> dataTablesResponse =
                Assert.IsAssignableFrom<DataTablesResponse<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Empty(dataTablesResponse.Data);
            Assert.Equal(0, dataTablesResponse.RecordsTotal);
        }

        [Fact]
        public async Task GetAllCuisines_Positive_SearchIsNull_ShouldReturnAllCuisines()
        {
            List<CuisineDataModel> cuisineDataModelsMock = BuildCuisineDataModelsMock();
            DatatablesRequest datatablesRequestMock = BuildDatatablesRequestMock();
            // DataTables omits the "search" object entirely when no search box is bound,
            // which the controller has to tolerate without filtering anything out.
            datatablesRequestMock.Search = null!;

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisines(datatablesRequestMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            DataTablesResponse<CuisineViewModel> dataTablesResponse =
                Assert.IsAssignableFrom<DataTablesResponse<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Equal(3, dataTablesResponse.Data.Count);
            Assert.Equal(3, dataTablesResponse.RecordsTotal);
        }

        [Theory]
        // Sorting by Id - Western(1), Chinese(2), Malay(3).
        [InlineData("id", "asc", "Western,Chinese,Malay")]
        [InlineData("id", "desc", "Malay,Chinese,Western")]
        // Sorting by Name.
        [InlineData("name", "asc", "Chinese,Malay,Western")]
        [InlineData("name", "desc", "Western,Malay,Chinese")]
        // An unrecognised column falls through to the default branch, which sorts by Name.
        [InlineData("unknownColumn", "asc", "Chinese,Malay,Western")]
        [InlineData("unknownColumn", "desc", "Western,Malay,Chinese")]
        // A null direction is coalesced to "asc".
        [InlineData("name", null, "Chinese,Malay,Western")]
        public async Task GetAllCuisines_Positive_SortRequested_ShouldReturnCuisinesInExpectedOrder(
            string sortField,
            string? sortDirection,
            string expectedNames)
        {
            List<CuisineDataModel> cuisineDataModelsMock = BuildCuisineDataModelsMock();
            DatatablesRequest datatablesRequestMock =
                BuildDatatablesRequestMock(sortField: sortField, sortDirection: sortDirection);

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisines(datatablesRequestMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            DataTablesResponse<CuisineViewModel> dataTablesResponse =
                Assert.IsAssignableFrom<DataTablesResponse<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.NotEmpty(dataTablesResponse.Data);
            Assert.Equal(expectedNames, string.Join(",", dataTablesResponse.Data.Select(cuisine => cuisine.Name)));
        }

        [Fact]
        public async Task GetAllCuisines_Positive_PaginationRequested_ShouldReturnOnlyRequestedPage()
        {
            List<CuisineDataModel> cuisineDataModelsMock = BuildCuisineDataModelsMock();
            // Name ascending is Chinese, Malay, Western - so skipping 1 and taking 1 gives Malay.
            DatatablesRequest datatablesRequestMock = BuildDatatablesRequestMock(start: 1, length: 1);

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisines(datatablesRequestMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            DataTablesResponse<CuisineViewModel> dataTablesResponse =
                Assert.IsAssignableFrom<DataTablesResponse<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            CuisineViewModel cuisineViewModel = Assert.Single(dataTablesResponse.Data);
            Assert.Equal("Malay", cuisineViewModel.Name);
            // Paging must not shrink the totals, otherwise the grid loses its page count.
            Assert.Equal(3, dataTablesResponse.RecordsTotal);
            Assert.Equal(3, dataTablesResponse.RecordsFiltered);
        }

        [Fact]
        public async Task GetAllCuisines_Negative_ExceptionOccured_ShouldThrowApiException()
        {
            DatatablesRequest datatablesRequestMock = BuildDatatablesRequestMock();

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync())
                .Throws(new ApiException("Mocked exception occured."));

            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.GetAllCuisines(datatablesRequestMock));

            Assert.Equal("Mocked exception occured.", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        [Fact]
        public async Task GetAllCuisines_Negative_RequestIsNull_ShouldThrowApiException()
        {
            List<CuisineDataModel> cuisineDataModelsMock = BuildCuisineDataModelsMock();

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            // The search filter is null safe, but reading Order[0] afterwards is not - the
            // resulting NullReferenceException is caught and resurfaced as an ApiException.
            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.GetAllCuisines(null!));

            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        #endregion

        #region GetAllCuisineViewModel

        [Fact]
        public async Task GetAllCuisineViewModel_Negative_NoCuisineExists_ShouldReturnEmptyList()
        {

            List<CuisineDataModel> cuisineDataModelMock = new List<CuisineDataModel>();

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelMock);

            ApiResponse? result = await _cuisineController.GetAllCuisineViewModel();

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            List<CuisineViewModel> cuisineViewModels = Assert.IsAssignableFrom<List<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Empty(cuisineViewModels);
        }

        [Fact]
        public async Task GetAllCuisineViewModel_Positive_CuisineExists_ShouldReturnNonEmptyList()
        {
            CuisineDataModel cuisineDataModelMock = new CuisineDataModel
            {
                Id = Guid.NewGuid(),
                Name = "Chinese"
            };

            List<CuisineDataModel> cuisineDataModelsMock = new List<CuisineDataModel> { cuisineDataModelMock };

            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Returns(cuisineDataModelsMock);

            ApiResponse? result = await _cuisineController.GetAllCuisineViewModel();

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            List<CuisineViewModel> cuisineViewModels = Assert.IsAssignableFrom<List<CuisineViewModel>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.NotEmpty(cuisineViewModels);

            CuisineViewModel cuisineViewModel = Assert.Single(cuisineViewModels);
            Assert.Equal(cuisineDataModelMock.Id, cuisineViewModel.Id);
            Assert.Equal(cuisineDataModelMock.Name, cuisineViewModel.Name);
        }

        [Fact]
        public async Task GetAllCuisineViewModel_Negative_ExceptionOccured_ShouldThrowException()
        {
            A.CallTo(() => _cuisineServiceMock.GetAllCuisineAsync()).Throws(new ApiException("Mocked exception occured."));

            // The controller catches and rethrows, so this is a *new* ApiException that
            // only carries the original message forward - not the instance faked above.
            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.GetAllCuisineViewModel());

            Assert.Equal("Mocked exception occured.", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        #endregion

        #region GetCuisineById

        [Fact]
        public async Task GetCuisineById_Positive_CuisineExists_ShouldReturnCuisineViewModel()
        {
            CuisineDataModel cuisineDataModelMock = new CuisineDataModel
            {
                Id = _chineseIdMock,
                Name = "Chinese"
            };

            A.CallTo(() => _cuisineServiceMock.GetCuisineByIdProcessAsync(_chineseIdMock))
                .Returns(cuisineDataModelMock);

            ApiResponse? result = await _cuisineController.GetCuisineById(_chineseIdMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            CuisineViewModel cuisineViewModel = Assert.IsAssignableFrom<CuisineViewModel>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Equal(cuisineDataModelMock.Id, cuisineViewModel.Id);
            Assert.Equal(cuisineDataModelMock.Name, cuisineViewModel.Name);
        }

        [Fact]
        public async Task GetCuisineById_Negative_IdIsEmpty_ShouldThrowApiExceptionWithBadRequest()
        {
            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.GetCuisineById(Guid.Empty));

            Assert.Equal("Id is empty", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);

            // The guard must short circuit before the service is ever reached.
            A.CallTo(() => _cuisineServiceMock.GetCuisineByIdProcessAsync(A<Guid>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task GetCuisineById_Negative_CuisineNotFound_ShouldPropagateApiException()
        {
            A.CallTo(() => _cuisineServiceMock.GetCuisineByIdProcessAsync(_chineseIdMock))
                .Throws(new ApiException("No cuisine found", StatusCodes.Status404NotFound));

            // There is no try/catch in this action, so the service's exception surfaces as is.
            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.GetCuisineById(_chineseIdMock));

            Assert.Equal("No cuisine found", exception.Message);
            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        }

        #endregion

        #region CreateCuisine

        [Fact]
        public async Task CreateCuisine_Positive_ValidDto_ShouldReturnCreated()
        {
            CreateCuisineDto createCuisineDtoMock = new CreateCuisineDto { Name = "Japanese" };

            CuisineDataModel? capturedCuisineDataModelMock = null;
            A.CallTo(() => _cuisineServiceMock.CreateCuisineProcessAsync(A<CuisineDataModel>._))
                .Invokes((CuisineDataModel cuisineDataModel) => capturedCuisineDataModelMock = cuisineDataModel);

            ApiResponse? result = await _cuisineController.CreateCuisine(createCuisineDtoMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            Assert.Equal(StatusCodes.Status201Created, viewResult.StatusCode);
            Assert.Equal(StatusCodes.Status201Created, viewResult.Result);

            // The Dto -> DataModel map deliberately ignores Id, so the service must be
            // handed an empty Id and let the database assign one.
            Assert.NotNull(capturedCuisineDataModelMock);
            Assert.Equal("Japanese", capturedCuisineDataModelMock!.Name);
            Assert.Equal(Guid.Empty, capturedCuisineDataModelMock.Id);
        }

        [Fact]
        public async Task CreateCuisine_Negative_ExceptionOccured_ShouldThrowApiException()
        {
            CreateCuisineDto createCuisineDtoMock = new CreateCuisineDto { Name = "Japanese" };

            A.CallTo(() => _cuisineServiceMock.CreateCuisineProcessAsync(A<CuisineDataModel>._))
                .Throws(new ApiException("Duplicate record detected."));

            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.CreateCuisine(createCuisineDtoMock));

            Assert.Equal("Duplicate record detected.", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        #endregion

        #region EditCuisineAsync

        [Fact]
        public async Task EditCuisineAsync_Positive_ValidDto_ShouldReturnOk()
        {
            EditCuisineDto editCuisineDtoMock = new EditCuisineDto
            {
                Id = _chineseIdMock,
                Name = "Cantonese"
            };

            CuisineDataModel? capturedCuisineDataModelMock = null;
            A.CallTo(() => _cuisineServiceMock.UpdateCuisineProcessAsync(A<CuisineDataModel>._))
                .Invokes((CuisineDataModel cuisineDataModel) => capturedCuisineDataModelMock = cuisineDataModel);

            ApiResponse? result = await _cuisineController.EditCuisineAsync(editCuisineDtoMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Equal(StatusCodes.Status200OK, viewResult.Result);

            // Unlike the create map, the edit map must carry the Id through.
            Assert.NotNull(capturedCuisineDataModelMock);
            Assert.Equal(_chineseIdMock, capturedCuisineDataModelMock!.Id);
            Assert.Equal("Cantonese", capturedCuisineDataModelMock.Name);
        }

        [Fact]
        public async Task EditCuisineAsync_Negative_ExceptionOccured_ShouldThrowApiException()
        {
            EditCuisineDto editCuisineDtoMock = new EditCuisineDto
            {
                Id = _chineseIdMock,
                Name = "Cantonese"
            };

            A.CallTo(() => _cuisineServiceMock.UpdateCuisineProcessAsync(A<CuisineDataModel>._))
                .Throws(new ApiException("No cuisine found", StatusCodes.Status404NotFound));

            // The catch block rebuilds the exception, so the 404 collapses to the
            // ApiException default of 400 - only the message survives.
            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.EditCuisineAsync(editCuisineDtoMock));

            Assert.Equal("No cuisine found", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        #endregion

        #region RemoveCuisineAsync

        [Fact]
        public async Task RemoveCuisineAsync_Positive_CuisineDeleted_ShouldReturnOk()
        {
            ApiResponse? result = await _cuisineController.RemoveCuisineAsync(_chineseIdMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Equal("Cuisine Deleted Successfully", viewResult.Message);

            A.CallTo(() => _cuisineServiceMock.DeleteCuisineProcessAsync(_chineseIdMock))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RemoveCuisineAsync_Negative_ExceptionOccured_ShouldThrowApiException()
        {
            A.CallTo(() => _cuisineServiceMock.DeleteCuisineProcessAsync(_chineseIdMock))
                .Throws(new ApiException("No cuisine found", StatusCodes.Status404NotFound));

            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.RemoveCuisineAsync(_chineseIdMock));

            Assert.Equal("No cuisine found", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        #endregion

        #region ExcelUpload

        [Fact]
        public async Task ExcelUpload_Positive_RowsUploaded_ShouldReturnNonEmptyResultList()
        {
            List<UploadCuisineExcelData> uploadCuisineExcelDatasMock = new List<UploadCuisineExcelData>
            {
                new UploadCuisineExcelData { EntityName = "Japanese", TotalCount = 1 }
            };

            List<UploadExcelResult> uploadExcelResultsMock = new List<UploadExcelResult>
            {
                new UploadExcelResult { EntityName = "Japanese", IsValid = true }
            };

            ApiResponse apiResponseMock =
                new ApiResponse("1 cuisine(s) successfully uploaded.", uploadExcelResultsMock, StatusCodes.Status200OK);

            A.CallTo(() => _cuisineServiceMock.CuisineExcelUploadProcessAsync(uploadCuisineExcelDatasMock))
                .Returns(apiResponseMock);

            ApiResponse? result = await _cuisineController.ExcelUpload(uploadCuisineExcelDatasMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            List<UploadExcelResult> uploadExcelResults =
                Assert.IsAssignableFrom<List<UploadExcelResult>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.NotEmpty(uploadExcelResults);

            UploadExcelResult uploadExcelResult = Assert.Single(uploadExcelResults);
            Assert.Equal("Japanese", uploadExcelResult.EntityName);
            Assert.True(uploadExcelResult.IsValid);
        }

        [Fact]
        public async Task ExcelUpload_Negative_NoRowsUploaded_ShouldReturnEmptyResultList()
        {
            List<UploadCuisineExcelData> uploadCuisineExcelDatasMock = new List<UploadCuisineExcelData>();

            ApiResponse apiResponseMock = new ApiResponse("No cuisine(s) have been uploaded. Excel sheet rows are empty.",
                                                         new List<UploadExcelResult>(),
                                                         StatusCodes.Status200OK);

            A.CallTo(() => _cuisineServiceMock.CuisineExcelUploadProcessAsync(uploadCuisineExcelDatasMock))
                .Returns(apiResponseMock);

            ApiResponse? result = await _cuisineController.ExcelUpload(uploadCuisineExcelDatasMock);

            ApiResponse viewResult = Assert.IsType<ApiResponse>(result);

            List<UploadExcelResult> uploadExcelResults =
                Assert.IsAssignableFrom<List<UploadExcelResult>>(viewResult.Result);

            Assert.Equal(StatusCodes.Status200OK, viewResult.StatusCode);
            Assert.Empty(uploadExcelResults);
        }

        [Fact]
        public async Task ExcelUpload_Negative_ExceptionOccured_ShouldPropagateApiException()
        {
            List<UploadCuisineExcelData> uploadCuisineExcelDatasMock = new List<UploadCuisineExcelData>
            {
                new UploadCuisineExcelData { EntityName = "Japanese", TotalCount = 1 }
            };

            A.CallTo(() => _cuisineServiceMock.CuisineExcelUploadProcessAsync(uploadCuisineExcelDatasMock))
                .Throws(new ApiException("Mocked exception occured.", StatusCodes.Status500InternalServerError));

            // This action has no try/catch, so the service's exception is not reshaped.
            ApiException exception = await Assert.ThrowsAsync<ApiException>(
                () => _cuisineController.ExcelUpload(uploadCuisineExcelDatasMock));

            Assert.Equal("Mocked exception occured.", exception.Message);
            Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        }

        #endregion
    }
}

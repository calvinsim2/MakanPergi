using Common;
using MakanPergi.Models;
using MakanPergi.Models.Modals;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Refit;
using System.Net;

namespace MakanPergi.Common
{
    public class ApiResponseActionResult<T> : IActionResult
    {
        private readonly ApiResponse<ResultWrapper<T>> _apiResponse;
        private readonly string _customMessage = "Something went wrong.";

        public ApiResponseActionResult(ApiResponse<ResultWrapper<T>> apiResponse)
        {
            _apiResponse = apiResponse;
        }

        public ApiResponseActionResult(ApiResponse<ResultWrapper<T>> apiResponse, bool readErrorMsg)
        {
            _apiResponse = apiResponse;
            if (apiResponse == null || !apiResponse.IsSuccessStatusCode || apiResponse.Content == null)
            {
                Console.WriteLine($"apiResponseError: {apiResponse.Error.Content}");
                if (apiResponse != null && apiResponse.Error?.Content != null)
                {
                    try
                    {
                        BadRequestModel? errorModel = JsonConvert.DeserializeObject<BadRequestModel>(apiResponse.Error.Content);
                        string? message = ExtractMessage(errorModel?.responseException?.exceptionMessage);
                        if (!string.IsNullOrEmpty(message))
                        {
                            _customMessage = message;
                        }
                    }
                    catch (JsonException)
                    {
                        // Unexpected error shape - keep the default "Something went wrong."
                    }
                }
            }
        }

        private static string? ExtractMessage(JToken? exceptionMessage)
        {
            if (exceptionMessage == null) return null;

            // Our own ApiException: "exceptionMessage": "Simulated Failure!"
            if (exceptionMessage.Type == JTokenType.String) return exceptionMessage.ToString();

            // Model validation: "exceptionMessage": { "errors": { "Address": ["..."] } }
            if (exceptionMessage["errors"] is JObject errors)
            {
                return string.Join(" ", errors.Properties().SelectMany(p => p.Value.Values<string>()));
            }

            return exceptionMessage["title"]?.ToString();
        }

        public ApiResponseActionResult(ApiResponse<ResultWrapper<T>> apiResponse, string customMessage)
        {
            _apiResponse = apiResponse;
            _customMessage = customMessage;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            if (!_apiResponse.IsSuccessStatusCode)
            {
                var content = _customMessage;
                switch (_apiResponse.StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        content = "Conflict occured.";
                        break;
                    case HttpStatusCode.UnprocessableEntity:
                        content = "Duplicate record found.";
                        break;
                }

                var contentResult = new ContentResult()
                {
                    Content = content,
                    StatusCode = (int?)_apiResponse.StatusCode
                };

                await contentResult.ExecuteResultAsync(context);
            }
            else
            {
                var okResult = new ContentResult()
                {
                    Content = _apiResponse.Content.ToJsonString(),
                    StatusCode = (int?)_apiResponse.StatusCode
                };

                await okResult.ExecuteResultAsync(context);
            }
        }
    }
}

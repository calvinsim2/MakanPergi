using Newtonsoft.Json.Linq;

namespace MakanPergi.Models.Modals
{
    // using AutoWrapper at backend, it returns in this format
    // {
          //"isError": true,
          //"responseException": {
                // "exceptionMessage": "No cuisine found"
          //}
    // }

    // thus, we model our BadRequestModel in this manner.
    public class BadRequestModel
    {
        public bool isError { get; set; }
        public ResponseException responseException { get; set; }
    }

    public class ResponseException
    {
        // string when we throw ApiException ourselves,
        // object when [ApiController] model validation fails before the action runs
        public JToken? exceptionMessage { get; set; }
    }
}

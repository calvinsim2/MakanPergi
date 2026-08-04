using Common.Helpers;
using Common.Model;
using System.Text;

namespace MakanPergiWebApi.Controllers
{
    public interface IPortalService
    {
        Task SendMessage(WebSocketMsg msg, CancellationToken cancellationToken = default);
    }

    public class PortalService : IPortalService
    {
        private readonly ILogger<PortalService> _logger;
        private readonly IConfiguration _configuration;
        public PortalService(ILogger<PortalService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendMessage(WebSocketMsg msg, CancellationToken cancellationToken = default)
        {
            try
            {
                var portalUrl = _configuration["ApiSetting:PortalUrl"]?.ToString();
                using var httpClient = MyHttpUtility.GetPortalHttpClient();

                var url = $"{portalUrl}/api/PortalNotifyApi/sendMessage";
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(msg);

                _logger.LogInformation("PortalUrl = {PortalUrl}", portalUrl);
                _logger.LogInformation("Calling Portal notify URL = {Url}", url);

                var result = await httpClient.PostAsync(
                    url,
                    new StringContent(json, Encoding.UTF8, "application/json"),
                    cancellationToken);

                _logger.LogInformation("Portal notify response status = {StatusCode}", result.StatusCode);

                var body = await result.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Portal notify response body = {Body}", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Portal notify failed");
                throw;
            }
        }
    }
}

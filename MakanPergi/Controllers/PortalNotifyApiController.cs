using Common.Model;
using MakanPergi.Infrastructure.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MakanPergi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortalNotifyApiController : ControllerBase
    {
        private readonly ILogger<PortalNotifyApiController> _logger;
        private IHubContext<MainHub> _hub;

        public PortalNotifyApiController(ILogger<PortalNotifyApiController> logger, IHubContext<MainHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage(WebSocketMsg message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("SendMessage invoked.");
            _logger.LogInformation("Before hub SendAsync. Action = {Action}", message.Action);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            await _hub.Clients.All.SendAsync(message.Action, message.JsonMessageBody, cancellationToken);

            sw.Stop();
            _logger.LogInformation("After hub SendAsync. ElapsedMs = {ElapsedMs}", sw.ElapsedMilliseconds);

            return Ok();
        }
    }
}

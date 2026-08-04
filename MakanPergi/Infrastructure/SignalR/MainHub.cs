using Microsoft.AspNetCore.SignalR;

namespace MakanPergi.Infrastructure.SignalR
{
    public class MainHub : Hub<IMainHub>
    {
        private readonly ILogger<MainHub> _logger;
        //private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly ILoginUserCollectionService? _loginUserCollectionService;

        public MainHub(ILogger<MainHub> logger)
        {
            _logger = logger;
            //_httpContextAccessor = httpContextAccessor;
            //_loginUserCollectionService = httpContextAccessor.HttpContext?.RequestServices.GetService<ILoginUserCollectionService>();
        }

        public override async Task OnConnectedAsync()
        {
            //var userID = _httpContextAccessor.HttpContext?.Session.GetSessionByKey<string>(CERTIS.CEMS.AdminPortal.Common.Constants.Constants.User);
            //var userName = _httpContextAccessor.HttpContext?.Session.GetSessionByKey<string>(CERTIS.CEMS.AdminPortal.Common.Constants.Constants.UserName);
            //if (_loginUserCollectionService != null && !string.IsNullOrWhiteSpace(userID) && Guid.TryParse(userID, out Guid userGuid))
            //{
            //    _loginUserCollectionService.AddLoginUser(userGuid, userName);
            //}
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //var userID = _httpContextAccessor.HttpContext?.Session.GetSessionByKey<string>(CERTIS.CEMS.AdminPortal.Common.Constants.Constants.User);
            //if (_loginUserCollectionService != null && !string.IsNullOrWhiteSpace(userID) && Guid.TryParse(userID, out Guid userGuid))
            //{
            //    _loginUserCollectionService.RemoveLoginUser(userGuid);
            //}
            await base.OnDisconnectedAsync(exception);
        }

        //private void AddUserConnectionMapping()
        //{
        //    string connectionId = Context.ConnectionId;
        //    string username = String.IsNullOrWhiteSpace(Context.User.Identity.Name) ? "Unknown" : Context.User.Identity.Name;

        //    var hubUser = new EntryStnHubUser()
        //    {
        //        UserName = username
        //    };
        //    //_connectionMappings.TryAdd(connectionId, hubUser);
        //    _logger.LogInformation($"Connection added: {connectionId} for {hubUser.UserName}");
        //}

        //private async Task<string> QuitGroup(string? locationGrpId)
        //{
        //    var grpname = locationGrpId ?? Guid.Empty.ToString();
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, grpname);
        //    return $"This connection has quitted - Group({grpname})";
        //}

        //public async Task<string> JoinGroup(string? locationGrpId)
        //{
        //    Console.WriteLine($"JoinGroup: {locationGrpId}");
        //    var grpname = locationGrpId ?? Guid.Empty.ToString();
        //    await Groups.AddToGroupAsync(Context.ConnectionId, grpname);
        //    return $"This connection has joined - Group({grpname})";
        //}

        public async Task<string> GetConnID()
        {
            return await Task.Run(() => {
                return Context.ConnectionId;
            });
        }
    }
}

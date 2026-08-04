using AutoMapper;
using MakanPergi;
using MakanPergi.Common.AppConfig;
using MakanPergi.Common.Helpers;
using MakanPergi.Infrastructure.SignalR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ApiSetting apiSetting = builder.Configuration.GetSection("ApiSetting").Get<ApiSetting>();
var apiSettingCheck = builder.Configuration.GetSection("ApiSetting").Get<ApiSetting>();
//Console.WriteLine("ApiSetting loaded: " + (apiSettingCheck != null));
//Console.WriteLine("ApiSetting.UrlItems count: " + (apiSettingCheck?.UrlItems?.Count ?? 0));
// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile(new GlobalProfile());
});
builder.Services.AddControllersWithViews();
builder.Services.AddDependencies(apiSetting);
try
{
    var provider = builder.Services.BuildServiceProvider();
    var mapper = provider.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}
catch (Exception ex)
{
    //Console.WriteLine("AutoMapper config failed: " + ex.Message);
    if (ex is AggregateException aggEx)
    {
        foreach (var inner in aggEx.InnerExceptions)
        {
            Console.WriteLine(">> " + inner.Message);
        }
    }
    else
    {
        Console.WriteLine(">> " + ex.InnerException?.Message);
    }

    throw; // rethrow if needed
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<MainHub>("/MainHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

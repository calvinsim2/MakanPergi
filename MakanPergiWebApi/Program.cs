using AutoWrapper;
using Common.BackendJobs;
using Hangfire;
using Hangfire.Dashboard;
using MakanPergi.Data;
using MakanPergiWebApi.Controllers;
using MakanPergiWebApi.Helpers;
using MakanPergiWebApi.Infrastructure.Jobs;
using MakanPergiWebApi.Interfaces.Repository;
using MakanPergiWebApi.Interfaces.Services;
using MakanPergiWebApi.Repository;
using MakanPergiWebApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var assembly = Assembly.GetExecutingAssembly();

// we will do this step AFTER we created our DbContext
// Context is the db context that we created.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MakanPergiDbContext>(options => options.UseSqlServer(connectionString));
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
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy", builder =>
    {
        builder.AllowAnyOrigin();
        // builder.WithOrigins(<insert ur frontend url here>, it accepts a string of array.)
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile(new GlobalProfile());
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
});

builder.Services.AddScoped<IDbContext, MakanPergiDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICuisineService, CuisineService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IPortalService, PortalService>();

builder.Services.AddHangfire(c => c.UseInMemoryStorage());

builder.Services.AddHangfireServer();

var hangFireJobsCollection = new Dictionary<string, Type>
{
    { typeof(DemoBatchJob).Name, typeof(DemoBatchJobRequest) }
};

builder.Services.AddBackendJobConfig(builder.Configuration, hangFireJobsCollection);

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

// Force HangfireJob constructor to run
app.Services.GetRequiredService<IBackendJob>();

app.UseSwagger();
app.UseSwaggerUI();

// using AutoWrapper v4.5.0 or earlier, our Apis can just return ApiResponse, it wouldn't cause double wrapping.
// but, any later versions, our Controller shouldn't return ApiResponse, it will cause double wrapping.
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions()
{
    IsApiOnly = false
});

app.UseHttpsRedirection();
app.UseCors("MyPolicy");

app.UseAuthorization();

app.MapControllers();

var options = new DashboardOptions
{
    Authorization = new[]
            {
                new LocalRequestsOnlyAuthorizationFilter()
            },
    IsReadOnlyFunc = (context) => true
};

app.UseHangfireDashboard("/hangfire", options);

app.Run();

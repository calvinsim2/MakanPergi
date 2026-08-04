using MakanPergi.Common.AppConfig;
using MakanPergi.Common.Constants;
using MakanPergi.Interfaces;
using Refit;

namespace MakanPergi
{
    public static class DependencyRegistry
    {
        public static void AddDependencies(this IServiceCollection services, ApiSetting apis)
        {
            if (apis is null || apis.UrlItems is null || !apis.UrlItems.Any())
            {
                throw new ArgumentNullException(nameof(apis));
            }

            foreach (UrlItem item in apis.UrlItems) 
            {
                Console.WriteLine($"UrlItem Name: {item.Name}");
                switch (item.Name)
                {
                    case Constants.MakanPergiWebApi:
                        Console.WriteLine("Matched MakanPergiWebApi, registering ICuisineService");
                        services.AddRefitClient<ICuisineService>()
                            .ConfigureHttpClient( c => c.BaseAddress = new Uri(item.Url));
                        Console.WriteLine("Matched MakanPergiWebApi, registering ICategoryService");
                        services.AddRefitClient<ICategoryService>()
                            .ConfigureHttpClient(c => c.BaseAddress = new Uri(item.Url));
                        Console.WriteLine("Matched MakanPergiWebApi, registering IFoodService");
                        services.AddRefitClient<IFoodService>()
                            .ConfigureHttpClient(c => c.BaseAddress = new Uri(item.Url));
                        Console.WriteLine("Matched MakanPergiWebApi, registering IShopService");
                        services.AddRefitClient<IShopService>()
                            .ConfigureHttpClient(c => c.BaseAddress = new Uri(item.Url));

                        break;
                }
            }
        }
    }
}

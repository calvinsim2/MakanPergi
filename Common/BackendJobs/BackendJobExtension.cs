using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.BackendJobs
{
    public static class BackendJobExtension
    {
        public static IServiceCollection AddBackendJobConfig(this IServiceCollection services, IConfiguration configuration, Dictionary<string, Type> _serviceCollection)
        {
            var options = new BackendJobOptions();

            configuration.GetSection(nameof(BackendJobOptions)).Bind(options);
            services.Configure<BackendJobOptions>(configuration.GetSection(nameof(BackendJobOptions)));
            services.AddSingleton<IBackendJob, HangfireJob>();
            services.AddSingleton(implementationFactory =>
            {
                Func<string, IRequest?> accesor = (key) =>
                {
                    //if (_serviceCollection.ContainsKey(key))
                    //{
                    //    var service = implementationFactory.CreateScope().ServiceProvider.GetService(_serviceCollection[key]);
                    //    if (service != null && service is IRequest)
                    //    {
                    //        return (IRequest)service;
                    //    }
                    //}

                    if (_serviceCollection.ContainsKey(key))
                    {
                        var requestType = _serviceCollection[key];

                        if (typeof(IRequest).IsAssignableFrom(requestType))
                        {
                            return Activator.CreateInstance(requestType) as IRequest;
                        }
                    }
                    return null;
                };
                return accesor;
            });
            return services;
        }
    }
}

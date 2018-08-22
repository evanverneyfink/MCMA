using System;
using Mcma.Azure.Http;
using Mcma.Server;
using Mcma.Server.Api;
using Mcma.Server.AuthorizedUrls;
using Mcma.Server.Business;
using Mcma.Server.Data;
using Mcma.Server.Environment;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Startup
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMcmaAzure(this IServiceCollection services, Action<IConfigurationBuilder> addConfig = null)
        {
            // build config
            var configBuilder = new ConfigurationBuilder();
            addConfig?.Invoke(configBuilder);
            var config = configBuilder.Build();

            // add logging and config
            services
                .Configure<LoggerOptions>(opts => opts.LogToConsole = true)
                .AddSingleton<ILogger, MicrosoftLoggerWrapper>()
                .AddEnvironment(opts => opts.AddProvider(new ConfigValueProvider(config))
                                            .AddProviderFactory(AzureEnvironment.GetPublicUrlProvider))
                .AddMcmaResourceDataHandling()
                .AddAuthorizedUrlBuilding()
                .AddProviderAuthorizedUrlBuilder<AzureCodeAuthorizerUrlProvider>();

            return services;
        }

        /// <summary>
        /// Registers resource API-related services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="resourceHandlerRegistration"></param>
        /// <param name="addConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddMcmaResourceApi(this IServiceCollection services,
                                                            IResourceHandlerRegistration resourceHandlerRegistration,
                                                            Action<IConfigurationBuilder> addConfig = null)
            => services
               .AddMcmaAzure(addConfig)
               .AddMcmaResourceHandling(resourceHandlerRegistration)
               .AddMcmaServerDefaultApi()
               .AddScoped<IRequest, HttpRequestWrapper>()
               .AddScoped<IMcmaAzureResourceApi, McmaAzureResourceApi>();

        /// <summary>
        /// Registers resource API-related services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="addConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddMcmaResourceApi<T>(this IServiceCollection services, Action<IConfigurationBuilder> addConfig = null)
            where T : class, IResourceHandlerRegistration, new()
            => services.AddMcmaResourceApi(new T(), addConfig);

        /// <summary>
        /// Registers resource API-related services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="addConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddMcmaWorker<T>(this IServiceCollection services, Action<IConfigurationBuilder> addConfig = null)
            where T : class, IWorker
            => services
               .AddMcmaAzure(addConfig)
               .AddScoped<IWorker, T>()
               .AddScoped<IMcmaAzureWorker, McmaAzureWorker>();
    }
}
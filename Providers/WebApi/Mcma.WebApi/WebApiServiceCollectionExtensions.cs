using Mcma.Server.Api;
using Mcma.Server.Business;
using Mcma.Server.Data;
using Mcma.Server.Environment;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.WebApi
{
    public static class WebApiServiceCollectionExtensions
    {
        public static IServiceCollection AddMcmaWebApi<T>(this IServiceCollection services, IConfiguration configuration)
            where T : IResourceHandlerRegistration, new()
        {
            return services.AddEnvironment(opts => opts.AddIisExpressUrl().AddProvider(new WebApiConfigValueProvider(configuration)))
                           .AddMcmaResourceDataHandling()
                           .AddMcmaResourceHandling<T>()
                           .AddMcmaServerDefaultApi()
                           .AddScoped<IRequest, WebApiRequest>();
        }

        public static IServiceCollection AddMcmaWorkerFunctionWebApi<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class, IWorkerFunctionInvoker
        {
            return services.AddEnvironment(opts => opts.AddIisExpressUrl().AddProvider(new WebApiConfigValueProvider(configuration)))
                           .AddMcmaResourceDataHandling()
                           .AddMcmaWorkerFunctionApi<T>()
                           .AddMcmaServerDefaultApi()
                           .AddScoped<IRequest, WebApiRequest>();
        }

        public static IServiceCollection AddMcmaInProcessWorkerFunctionWebApi<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class, IWorker
        {
            return services.AddEnvironment(opts => opts.AddIisExpressUrl().AddProvider(new WebApiConfigValueProvider(configuration)))
                           .AddMcmaResourceDataHandling()
                           .AddMcmaInProcesssWorkerFunctionApi<T>()
                           .AddMcmaServerDefaultApi()
                           .AddScoped<IRequest, WebApiRequest>();
        }
    }
}
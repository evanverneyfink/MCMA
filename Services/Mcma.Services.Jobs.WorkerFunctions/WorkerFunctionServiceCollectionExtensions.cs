using Mcma.Server.Business;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Services.Jobs.WorkerFunctions
{
    public static class WorkerFunctionServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the FIMS worker function API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddMcmaWorkerFunctionApi<T>(this IServiceCollection serviceCollection)
            where T : class, IWorkerFunctionInvoker
        {
            return serviceCollection.AddMcmaResourceHandling<WorkerFunctionInvocation<T>>();
        }
        
        /// <summary>
        /// Adds the FIMS worker function API with in-process worker processing
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddMcmaInProcesssWorkerFunctionApi<T>(this IServiceCollection serviceCollection) where T : class, IWorker
        {
            return serviceCollection
                   .AddMcmaResourceHandling(new WorkerFunctionInvocation(svcProvider => new InProcessWorkerFunctionInvoker(svcProvider.GetService)))
                   .AddScoped<T>();
        }
    }
}
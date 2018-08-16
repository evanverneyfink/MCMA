using Mcma.Azure.Startup;
using Mcma.Extensions.Files.AzureFileStorage;
using Mcma.Extensions.Repositories.AzureTableStorage;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.Ame.MediaInfo
{
    public class Startup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            =>
                services.AddMcmaResourceApi<WorkerFunctionInvocation<AzureFunctionWorkerInvoker>>()
                        .AddAzureFileStorage(opts => opts.FromEnvironmentVariables())
                        .AddAzureTableStorageRepository(opts => opts.FromEnvironmentVariables());
    }
}

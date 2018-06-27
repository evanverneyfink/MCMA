using Mcma.Azure.Startup;
using Mcma.Extensions.Files.AzureFileStorage;
using Mcma.Extensions.Repositories.AzureTableStorage;
using Mcma.Services.Ame.MediaInfo;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.Ame.MediaInfo
{
    public class MediaInfoStartup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaResourceApi<WorkerFunctionInvocation<AzureFunctionWorkerInvoker>>()
                       .AddMcmaWorker<MediaInfoWorker>()
                       .AddAzureTableStorageRepository()
                       .AddAzureFileStorage();
    }
}
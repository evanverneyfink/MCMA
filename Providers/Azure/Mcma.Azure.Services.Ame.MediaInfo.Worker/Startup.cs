using Mcma.Azure.Startup;
using Mcma.Extensions.Files.AzureFileStorage;
using Mcma.Extensions.Repositories.AzureTableStorage;
using Mcma.Services.Ame.MediaInfo;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.Ame.MediaInfo.Worker
{
    public class Startup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaWorker<MediaInfoWorker>()
                       .AddAzureFileStorage(opts => opts.FromEnvironmentVariables())
                       .AddAzureTableStorageRepository(opts => opts.FromEnvironmentVariables())
                       .AddMediaInfo<AzureMediaInfoAccessibleLocationProvider, AzureProcessLocator>();
    }
}
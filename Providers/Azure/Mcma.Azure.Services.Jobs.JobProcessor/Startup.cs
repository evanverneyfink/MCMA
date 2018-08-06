using Microsoft.Extensions.DependencyInjection;
using Mcma.Azure.Startup;
using Mcma.Extensions.Files.AzureFileStorage;
using Mcma.Extensions.Repositories.AzureTableStorage;

namespace Mcma.Azure.Services.Jobs.JobProcessor
{
    public class Startup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaResourceApi<Mcma.Services.Jobs.JobProcessor.JobProcessor>()
                       .AddAzureTableStorageRepository(opts => opts.FromEnvironmentVariables())
                       .AddAzureFileStorage(opts => opts.FromEnvironmentVariables());
    }
}

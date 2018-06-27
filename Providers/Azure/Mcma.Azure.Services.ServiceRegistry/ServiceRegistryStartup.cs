using Mcma.Azure.Startup;
using Mcma.Extensions.Files.AzureFileStorage;
using Mcma.Extensions.Repositories.AzureTableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.ServiceRegistry
{
    public class ServiceRegistryStartup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaResourceApi<Mcma.Services.ServiceRegistry.ServiceRegistry>()
                       .AddAzureTableStorageRepository()
                       .AddAzureFileStorage();
    }
}

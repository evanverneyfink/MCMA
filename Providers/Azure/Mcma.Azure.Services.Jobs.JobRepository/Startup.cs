using Mcma.Azure.Startup;
using Mcma.Extensions.Files.AzureFileStorage;
using Mcma.Extensions.Repositories.AzureTableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.Jobs.JobRepository
{
    public class Startup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaResourceApi<Mcma.Services.Jobs.JobRepository.JobRepository>()
                       .AddAzureTableStorageRepository(opts => opts.FromEnvironmentVariables())
                       .AddAzureFileStorage(opts => opts.FromEnvironmentVariables());
    }
}
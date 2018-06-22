using Mcma.Azure.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.Jobs.JobRepository
{
    public class JobRepositoryStartup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaResourceApi<Mcma.Services.Jobs.JobRepository.JobRepository>();
    }
}
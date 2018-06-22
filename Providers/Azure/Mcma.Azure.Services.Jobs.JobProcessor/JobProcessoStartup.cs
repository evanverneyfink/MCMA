using Mcma.Azure.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.Jobs.JobProcessor
{
    public class JobProcessoStartup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaResourceApi<Mcma.Services.Jobs.JobProcessor.JobProcessor>();
    }
}

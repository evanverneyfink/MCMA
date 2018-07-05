using Mcma.Azure.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace McmaServiceTemplate
{
    public class Startup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services) => services.AddMcmaResourceApi<ResourceApiRegistration>();
    }
}

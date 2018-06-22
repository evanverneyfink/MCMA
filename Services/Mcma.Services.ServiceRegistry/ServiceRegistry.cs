using Mcma.Core.Model;
using Mcma.Server.Business;

namespace Mcma.Services.ServiceRegistry
{
    public class ServiceRegistry : IResourceHandlerRegistration
    {
        public void Register(ResourceHandlerRegistryOptions options)
        {
            options.Register<Service>()
                   .Register<JobProfile>();
        }
    }
}

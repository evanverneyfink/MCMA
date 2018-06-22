using Mcma.Core.Model;
using Mcma.Server.Business;

namespace Mcma.Services.Jobs.JobProcessor
{
    public class JobProcessor : IResourceHandlerRegistration
    {
        public void Register(ResourceHandlerRegistryOptions opts)
        {
            opts.Register<JobProcess, JobProcessorResourceHandler>();
        }
    }
}

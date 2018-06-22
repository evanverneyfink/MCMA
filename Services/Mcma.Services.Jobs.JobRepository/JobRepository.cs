using Mcma.Core.Model;
using Mcma.Server.Business;

namespace Mcma.Services.Jobs.JobRepository
{
    public class JobRepository : IResourceHandlerRegistration
    {
        /// <summary>
        /// Adds resource handling for a FIMS job repository
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public void Register(ResourceHandlerRegistryOptions options)
        {
            options.Register<Job, JobRepositoryResourceHandler>();
        }
    }
}

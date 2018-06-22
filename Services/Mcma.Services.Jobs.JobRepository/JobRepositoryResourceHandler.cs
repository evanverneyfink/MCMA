using System;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;
using Mcma.Server;
using Mcma.Server.Api;
using Mcma.Server.Business;
using Mcma.Server.Data;
using Mcma.Server.Environment;

namespace Mcma.Services.Jobs.JobRepository
{
    public class JobRepositoryResourceHandler : ResourceHandler<Job>
    {
        /// <summary>
        /// Instantiates a <see cref="JobRepositoryResourceHandler"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="environment"></param>
        /// <param name="dataHandler"></param>
        /// <param name="resourceDescriptorHelper"></param>
        public JobRepositoryResourceHandler(ILogger logger,
                                            IEnvironment environment,
                                            IResourceDataHandler dataHandler,
                                            IResourceDescriptorHelper resourceDescriptorHelper)
            : base(logger, environment, dataHandler, resourceDescriptorHelper)
        {
        }

        /// <summary>
        /// Overridden to send job to job processor
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public override async Task<Job> Create(ResourceDescriptor resourceDescriptor, Job resource)
        {
            resource.JobStatus = "New";

            // create the job
            var job = await base.Create(resourceDescriptor, resource);

            // get all services
            var services = await DataHandler.Query<Service>(
                               ResourceDescriptor.FromUrl<Service>(Environment.ServiceRegistryUrl().TrimEnd('/') + "/Services"));

            // find first service that has a job processing endpoint
            var serviceResource =
                services
                    .SelectMany(s => s.HasResource.Select(r => new {Service = s, Resource = r}))
                    .FirstOrDefault(
                        sr => sr.Resource.ResourceType == nameof(JobProcess) && !string.IsNullOrWhiteSpace(sr.Resource.HttpEndpoint));

            if (serviceResource == null)
            {
                job.JobStatusReason = "No JobProcessor endpoints registered in the service registry.";
                job.JobStatus = "Failed";

                return await Update(resourceDescriptor, job);
            }

            try
            {
                // send the job to the job processing endpoint
                await DataHandler.Create(ResourceDescriptor.FromUrl<JobProcess>(serviceResource.Resource.HttpEndpoint),
                                         new JobProcess {Job = job});

                return await Get(resourceDescriptor);
            }
            catch (Exception ex)
            {
                job.JobStatusReason = $"Failed to send job to JobProcessor endpoint {serviceResource.Resource.HttpEndpoint}: {ex}";
                job.JobStatus = "Failed";

                return await Update(resourceDescriptor, job);
            }
        }
    }
}
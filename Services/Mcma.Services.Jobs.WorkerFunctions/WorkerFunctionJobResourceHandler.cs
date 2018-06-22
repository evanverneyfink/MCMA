using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;
using Mcma.Server;
using Mcma.Server.Api;
using Mcma.Server.Business;
using Mcma.Server.Data;
using Mcma.Server.Environment;

namespace Mcma.Services.Jobs.WorkerFunctions
{
    public class WorkerFunctionJobResourceHandler : ResourceHandler<JobAssignment>
    {
        /// <summary>
        /// Instantiates a <see cref="WorkerFunctionJobResourceHandler"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dataHandler"></param>
        /// <param name="resourceDescriptorHelper"></param>
        /// <param name="environment"></param>
        /// <param name="workerFunctionInvoker"></param>
        public WorkerFunctionJobResourceHandler(ILogger logger,
                                                IEnvironment environment,
                                                IResourceDataHandler dataHandler,
                                                IResourceDescriptorHelper resourceDescriptorHelper,
                                                IWorkerFunctionInvoker workerFunctionInvoker)
            : base(logger, environment, dataHandler, resourceDescriptorHelper)
        {
            WorkerFunctionInvoker = workerFunctionInvoker;
        }

        /// <summary>
        /// Gets the worker function invoker
        /// </summary>
        private IWorkerFunctionInvoker WorkerFunctionInvoker { get; }

        /// <summary>
        /// Gets a resource of type <see cref="JobAssignment"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public override async Task<JobAssignment> Create(ResourceDescriptor resourceDescriptor, JobAssignment resource)
        {
            // create the job assinment in the data layer
            var jobAssignment = await base.Create(resourceDescriptor, resource);

            // invoke worker
            await WorkerFunctionInvoker.Invoke(Environment.WorkerFunctionName(), Environment, resource);

            // return the newly-created job assignment
            return jobAssignment;
        }
    }
}

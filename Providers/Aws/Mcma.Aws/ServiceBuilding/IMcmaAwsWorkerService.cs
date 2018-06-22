using Mcma.Core.Serialization;
using Mcma.Server.Api;
using Mcma.Services.Jobs.WorkerFunctions;

namespace Mcma.Aws.ServiceBuilding
{
    public interface IMcmaAwsWorkerService : IMcmaService
    {
        /// <summary>
        /// Gets the worker
        /// </summary>
        IWorker Worker { get; }

        /// <summary>
        /// Gets the resource serializer
        /// </summary>
        IResourceSerializer ResourceSerializer { get; }
    }
}
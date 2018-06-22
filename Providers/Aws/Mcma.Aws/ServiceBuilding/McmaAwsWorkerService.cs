using System;
using Mcma.Core.Serialization;
using Mcma.Server;
using Mcma.Services.Jobs.WorkerFunctions;

namespace Mcma.Aws.ServiceBuilding
{
    public class McmaAwsWorkerService : IMcmaAwsWorkerService
    {
        /// <summary>
        /// Instantiates a <see cref="McmaAwsWorkerService"/>
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="logger"></param>
        /// <param name="worker"></param>
        /// <param name="resourceSerializer"></param>
        public McmaAwsWorkerService(IDisposable scope, ILogger logger, IWorker worker, IResourceSerializer resourceSerializer)
        {
            Scope = scope;
            Logger = logger;
            Worker = worker;
            ResourceSerializer = resourceSerializer;
        }

        /// <summary>
        /// Gets the scope
        /// </summary>
        private IDisposable Scope { get; }

        /// <summary>
        /// Gets the logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the worker
        /// </summary>
        public IWorker Worker { get; }

        /// <summary>
        /// Gets the resource serializer
        /// </summary>
        public IResourceSerializer ResourceSerializer { get; }

        /// <summary>
        /// Disposes of the underlying worker
        /// </summary>
        public void Dispose()
        {
            Scope?.Dispose();
        }
    }
}
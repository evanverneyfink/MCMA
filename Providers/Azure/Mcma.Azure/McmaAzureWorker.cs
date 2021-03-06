using System.IO;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
using Mcma.Server.Environment;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.AspNetCore.Http;

namespace Mcma.Azure
{
    public class McmaAzureWorker : IMcmaAzureWorker
    {
        /// <summary>
        /// Instantiates a <see cref="McmaAzureWorker"/>
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="resourceSerializer"></param>
        /// <param name="environment"></param>
        public McmaAzureWorker(IWorker worker, IResourceSerializer resourceSerializer, IEnvironment environment)
        {
            Worker = worker;
            ResourceSerializer = resourceSerializer;
            Environment = environment;
        }

        /// <summary>
        /// Gets the worker
        /// </summary>
        private IWorker Worker { get; }

        /// <summary>
        /// Gets the resource serializer
        /// </summary>
        private IResourceSerializer ResourceSerializer { get; }

        /// <summary>
        /// Gets the environment
        /// </summary>
        private IEnvironment Environment { get; }

        /// <summary>
        /// Executes a worker using the content provided in an HTTP request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async void DoWork(HttpRequest request)
        {
            await Worker.Execute(await ResourceSerializer.Deserialize<JobAssignment>(await new StreamReader(request.Body).ReadToEndAsync()));
        }
    }
}
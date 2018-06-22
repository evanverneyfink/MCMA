using System.IO;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
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
        public McmaAzureWorker(IWorker worker, IResourceSerializer resourceSerializer)
        {
            Worker = worker;
            ResourceSerializer = resourceSerializer;
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
        /// Executes a worker using the content provided in an HTTP request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task DoWork(HttpRequest request)
        {
            await Worker.Execute(await ResourceSerializer.Deserialize<JobAssignment>(await new StreamReader(request.Body).ReadToEndAsync()));
        }
    }
}
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
using Mcma.Server.Environment;
using Mcma.Services.Jobs.WorkerFunctions;

namespace Mcma.Azure
{
    public class AzureFunctionWorkerInvoker : IWorkerFunctionInvoker
    {
        /// <summary>
        /// Instantiates an <see cref="AzureFunctionWorkerInvoker"/>
        /// </summary>
        /// <param name="resourceSerializer"></param>
        /// <param name="environment"></param>
        public AzureFunctionWorkerInvoker(IResourceSerializer resourceSerializer, IEnvironment environment)
        {
            ResourceSerializer = resourceSerializer;
            Environment = environment;
        }

        /// <summary>
        /// Gets the resource sserializer
        /// </summary>
        private IResourceSerializer ResourceSerializer { get; }

        /// <summary>
        /// Gets the environment
        /// </summary>
        private IEnvironment Environment { get; }

        /// <summary>
        /// Gets the HTTP client used to invoke Azure Functions via HTTP
        /// </summary>
        private HttpClient HttpClient { get; } = new HttpClient();

        /// <summary>
        /// Invokes a worker function
        /// </summary>
        /// <param name="workerFunctionId"></param>
        /// <param name="environment"></param>
        /// <param name="jobAssignment"></param>
        /// <returns></returns>
        public async Task Invoke(string workerFunctionId, IEnvironment environment, JobAssignment jobAssignment)
        {
            await HttpClient.PostAsync(workerFunctionId,
                                       new StringContent(ResourceSerializer.Serialize(jobAssignment), Encoding.UTF8, "application/json"));
        }
    }
}
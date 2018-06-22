using System;
using System.Threading.Tasks;
using Amazon.Lambda;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
using Mcma.Server;
using Mcma.Server.Environment;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.Extensions.Options;

namespace Mcma.Aws.Lambda
{
    public class LambdaWorkerFunctionInvoker : IWorkerFunctionInvoker
    {
        /// <summary>
        /// Instantiates a <see cref="LambdaWorkerFunctionInvoker"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="resourceSerializer"></param>
        /// <param name="options"></param>
        public LambdaWorkerFunctionInvoker(ILogger logger, IResourceSerializer resourceSerializer, IOptions<LambdaOptions> options)
        {
            Logger = logger;
            ResourceSerializer = resourceSerializer;

            // create client using credentials, if provided
            var region = options.Value?.RegionEndpoint;
            var creds = options.Value?.Credentials;
            Lambda = creds != null ? new AmazonLambdaClient(creds, region) : new AmazonLambdaClient();
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the resource serializer
        /// </summary>
        private IResourceSerializer ResourceSerializer { get; }

        /// <summary>
        /// Gets the lambda client
        /// </summary>
        private IAmazonLambda Lambda { get; }

        /// <summary>
        /// Invokes a lambda worker
        /// </summary>
        /// <param name="workerFunctionId"></param>
        /// <param name="environment"></param>
        /// <param name="jobAssignment"></param>
        /// <returns></returns>
        public async Task Invoke(string workerFunctionId, IEnvironment environment, JobAssignment jobAssignment)
        {
            try
            {
                Logger.Info("Invoking lambda function with name '{0}' for job assignment {1}...", workerFunctionId, jobAssignment.Id);

                await Lambda.InvokeAsync(
                    new Amazon.Lambda.Model.InvokeRequest
                    {
                        FunctionName = workerFunctionId,
                        InvocationType = "Event",
                        LogType = "None",
                        Payload = ResourceSerializer.Serialize(jobAssignment)
                    });

                Logger.Info("Invocation of lambda function with name '{0}' for job assignment {1} succeeded.", workerFunctionId, jobAssignment.Id);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to invoke lambda function with name '{0}' for job assignment {1}. Exception: {2}",
                             workerFunctionId,
                             jobAssignment.Id,
                             ex);
                throw;
            }
        }
    }
}
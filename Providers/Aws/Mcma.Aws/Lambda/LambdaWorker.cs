using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Mcma.Aws.ServiceBuilding;
using Mcma.Core.Model;
using Mcma.Services.Jobs.WorkerFunctions;

namespace Mcma.Aws.Lambda
{
    public static class LambdaWorker
    {
        /// <summary>
        /// Runs a lambda worker
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="lambdaContext"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static async Task Handle<T>(Stream input, ILambdaContext lambdaContext, Action<McmaAwsServiceBuilder> configure = null) where T : class, IWorker
        {
            IMcmaAwsWorkerService service = null;
            try
            {
                // build worker service
                var serviceBuilder =
                    McmaAwsServiceBuilder.Create()
                                         .With(lambdaContext);

                configure?.Invoke(serviceBuilder);

                service = serviceBuilder.BuildWorkerSevice<T>();
                
                // read input as text and deserialize it
                var jobAssignment = await service.ResourceSerializer.Deserialize<JobAssignment>(await new StreamReader(input).ReadToEndAsync());

                // run worker
                await service.Worker.Execute(jobAssignment);
            }
            finally
            {
                service?.Dispose();
            }
        }
    }
}

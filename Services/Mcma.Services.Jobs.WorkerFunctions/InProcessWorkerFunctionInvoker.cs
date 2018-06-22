using System;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Server.Environment;

namespace Mcma.Services.Jobs.WorkerFunctions
{
    public class InProcessWorkerFunctionInvoker : IWorkerFunctionInvoker
    {
        /// <summary>
        /// Instantiates an <see cref="InProcessWorkerFunctionInvoker"/>
        /// </summary>
        /// <param name="resolver"></param>
        public InProcessWorkerFunctionInvoker(Func<Type, object> resolver)
        {
            Resolver = resolver;
        }

        /// <summary>
        /// Gets the worker resolver
        /// </summary>
        private Func<Type, object> Resolver { get; }

        /// <summary>
        /// Invokes a worker function in-process
        /// </summary>
        /// <param name="workerFunctionId"></param>
        /// <param name="environment"></param>
        /// <param name="jobAssignment"></param>
        /// <returns></returns>
        public Task Invoke(string workerFunctionId, IEnvironment environment, JobAssignment jobAssignment)
        {
            // get the worker using the function name
            var worker = GetWorker(workerFunctionId);

            // execute the worker on a background thread
            worker.Execute(jobAssignment);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the worker using the function name
        /// </summary>
        /// <param name="workerFunctionName"></param>
        /// <returns></returns>
        private IWorker GetWorker(string workerFunctionName)
        {
            var type = Type.GetType(workerFunctionName);
            if (type == null)
                throw new Exception(
                    $"Invalid worker function identifier {workerFunctionName}. Type {workerFunctionName} not found.");

            if (!typeof(IWorker).IsAssignableFrom(type))
                throw new Exception(
                    $"Invalid worker function identifier {workerFunctionName}. Type {type} does not implement the IWorker interface.");

            return (IWorker)Resolver(type);
        }
    }
}
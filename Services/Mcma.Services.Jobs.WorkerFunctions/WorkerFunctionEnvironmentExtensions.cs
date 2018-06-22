using Mcma.Server.Environment;

namespace Mcma.Services.Jobs.WorkerFunctions
{
    public static class WorkerFunctionEnvironmentExtensions
    {
        /// <summary>
        /// Gets the worker function name
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static string WorkerFunctionName(this IEnvironment env) => env.Get<string>(nameof(WorkerFunctionName));
    }
}
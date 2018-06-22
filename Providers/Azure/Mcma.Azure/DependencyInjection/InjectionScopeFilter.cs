using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace Mcma.Azure.DependencyInjection
{
    public class InjectionScopeFilter : IFunctionInvocationFilter, IFunctionExceptionFilter
    {
        /// <summary>
        /// Creates an <see cref="InjectionScopeFilter"/>
        /// </summary>
        /// <param name="scopeManager"></param>
        public InjectionScopeFilter(InjectionScopeManager scopeManager) => ScopeManager = scopeManager;

        /// <summary>
        /// Gets the underlying scope manager
        /// </summary>
        private InjectionScopeManager ScopeManager { get; }

        /// <summary>
        /// Handles start of function execution by registering a new scope
        /// </summary>
        /// <param name="executingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            ScopeManager.BeginScope(executingContext.FunctionInstanceId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles end of function execution by removing and disposing of a scope
        /// </summary>
        /// <param name="executedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            ScopeManager.EndScope(executedContext.FunctionInstanceId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles a function exception by removing and disposing of a scope
        /// </summary>
        /// <param name="exceptionContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            ScopeManager.EndScope(exceptionContext.FunctionInstanceId);
            return Task.CompletedTask;
        }
    }
}
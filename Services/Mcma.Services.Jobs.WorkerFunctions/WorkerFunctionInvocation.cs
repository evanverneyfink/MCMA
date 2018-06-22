using System;
using Mcma.Core.Model;
using Mcma.Server.Business;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Services.Jobs.WorkerFunctions
{
    public class WorkerFunctionInvocation : IResourceHandlerRegistration
    {
        public WorkerFunctionInvocation(Func<IServiceProvider, IWorkerFunctionInvoker> createWorkerFunctionInvoker)
            : this(services => services.AddScoped(createWorkerFunctionInvoker))
        {
        }

        protected WorkerFunctionInvocation(Action<IServiceCollection> addWorkerFunctionInvoker)
        {
            AddWorkerFunctionInvoker = addWorkerFunctionInvoker;
        }

        private Action<IServiceCollection> AddWorkerFunctionInvoker { get; }

        public void Register(ResourceHandlerRegistryOptions opts)
        {
            opts.Register<Job>()
                .Register<JobProcess>()
                .Register<JobAssignment, WorkerFunctionJobResourceHandler>();

            AddWorkerFunctionInvoker?.Invoke(opts.ServiceCollection);
        }
    }

    public class WorkerFunctionInvocation<T> : WorkerFunctionInvocation where T : class, IWorkerFunctionInvoker
    {
        public WorkerFunctionInvocation()
            : base(services => services.AddScoped<IWorkerFunctionInvoker, T>())
        {
        }
    }
}
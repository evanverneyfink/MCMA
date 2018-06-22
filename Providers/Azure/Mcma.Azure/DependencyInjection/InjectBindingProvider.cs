using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace Mcma.Azure.DependencyInjection
{
    public class InjectBindingProvider : IBindingProvider
    {
        /// <summary>
        /// Instantiates an <see cref="InjectBindingProvider"/>
        /// </summary>
        /// <param name="injectionScopeManager"></param>
        public InjectBindingProvider(IInjectionScopeManager injectionScopeManager)
        {
            InjectionScopeManager = injectionScopeManager;
        }

        /// <summary>
        /// Gets the injection scope manager
        /// </summary>
        private IInjectionScopeManager InjectionScopeManager { get; }

        /// <summary>
        /// Gets an <see cref="InjectBinding"/> fro a parameter
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            return Task.FromResult<IBinding>(new InjectBinding(InjectionScopeManager, context.Parameter));
        }
    }
}
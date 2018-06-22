using Mcma.Azure.Startup;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.DependencyInjection
{
    public class DependencyInjectionExtension<T> : IExtensionConfigProvider where T : IStartup, new()
    {
        /// <summary>
        /// Initializes the app by setting up dependency injection
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // allow derived classes to register services
            var services = new T().Configure(new ServiceCollection().AddSingleton(context.Config.LoggerFactory));

            // create injection scope manager
            var injectionScopeManager = new InjectionScopeManager(services.BuildServiceProvider(true));

            // create filter for managing scopes
            var injectionScopeFilter = new InjectionScopeFilter(injectionScopeManager);
            context.Config.RegisterExtension<IFunctionInvocationFilter>(injectionScopeFilter);
            context.Config.RegisterExtension<IFunctionExceptionFilter>(injectionScopeFilter);

            // map injection binding provider to the inject attribute
            context.AddBindingRule<InjectAttribute>().Bind(new InjectBindingProvider(injectionScopeManager));
        }
    }
}
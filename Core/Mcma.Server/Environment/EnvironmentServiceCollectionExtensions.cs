using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mcma.Server.Environment
{
    public static class EnvironmentServiceCollectionExtensions
    {
        /// <summary>
        /// Adds environment variables to the application
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddEnvironment(this IServiceCollection serviceCollection, Action<EnvironmentOptions> configureOptions = null)
        {
            // configure environment variable options
            var options = new EnvironmentOptions();
            configureOptions?.Invoke(options);
            serviceCollection.AddSingleton(options);

            // add environment to the 
            serviceCollection.TryAdd(ServiceDescriptor.Scoped<IEnvironment, Environment>());

            return serviceCollection;
        }
    }
}
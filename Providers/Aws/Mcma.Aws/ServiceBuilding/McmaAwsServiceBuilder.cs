using System;
using Mcma.Core.Serialization;
using Mcma.Server;
using Mcma.Server.Api;
using Mcma.Server.Business;
using Mcma.Server.Data;
using Mcma.Server.Environment;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Aws.ServiceBuilding
{
    public class McmaAwsServiceBuilder
    {
        /// <summary>
        /// Instantiates a <see cref="McmaAwsServiceBuilder"/>
        /// </summary>
        /// <param name="services"></param>
        private McmaAwsServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Gets the underlying service collection
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Creates a <see cref="McmaAwsServiceBuilder"/>
        /// </summary>
        /// <returns></returns>
        public static McmaAwsServiceBuilder Create(Action<EnvironmentOptions> configureEnvironmentVariables = null)
        {
            return new McmaAwsServiceBuilder(
                new ServiceCollection().AddMcmaResourceDataHandling()
                                       .AddEnvironment(configureEnvironmentVariables));
        }

        /// <summary>
        /// Adds an object to the service collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public McmaAwsServiceBuilder With<T>(T obj) where T : class
        {
            Services.AddScoped(x => obj);
            return this;
        }

        /// <summary>
        /// Adds an object to the service collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public McmaAwsServiceBuilder With<T>() where T : class
        {
            Services.AddScoped<T>();
            return this;
        }

        /// <summary>
        /// Adds a type registration
        /// </summary>
        /// <typeparam name="TRegistered"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public McmaAwsServiceBuilder With<TRegistered, TImplementation>()
            where TRegistered : class
            where TImplementation : class, TRegistered
        {
            Services.AddScoped<TRegistered, TImplementation>();
            return this;
        }

        /// <summary>
        /// Adds an object to the service collection
        /// </summary>
        /// <returns></returns>
        public McmaAwsServiceBuilder With(Action<IServiceCollection> register)
        {
            register(Services);
            return this;
        }

        /// <summary>
        /// Builds a resource API
        /// </summary>
        /// <returns></returns>
        public IMcmaAwsResourceApi BuildResourceApi<TRequestContext, TResourceRegistration>(Action<IServiceCollection> addAdditionalServices = null)
            where TRequestContext : class, IRequest
            where TResourceRegistration : IResourceHandlerRegistration, new()
        {
            Services
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddMcmaResourceHandling<TResourceRegistration>()
                .AddMcmaServerDefaultApi()
                .AddScoped<IRequest, TRequestContext>();

            addAdditionalServices?.Invoke(Services);

            var serviceProvider = Services.BuildServiceProvider();

            return new McmaAwsResourceApi(serviceProvider.CreateScope(),
                                          serviceProvider.GetRequiredService<ILogger>(),
                                          serviceProvider.GetRequiredService<IRequestHandler>());
        }

        /// <summary>
        /// Creates a builds a worker service
        /// </summary>
        /// <returns></returns>
        public IMcmaAwsWorkerService BuildWorkerSevice<T>(Action<IServiceCollection> addAdditionalServices = null)
            where T : class, IWorker
        {
            Services
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddScoped<IWorker, T>();

            addAdditionalServices?.Invoke(Services);

            var serviceProvider = Services.BuildServiceProvider();

            return new McmaAwsWorkerService(serviceProvider.CreateScope(),
                                            serviceProvider.GetRequiredService<ILogger>(),
                                            serviceProvider.GetRequiredService<IWorker>(),
                                            serviceProvider.GetRequiredService<IResourceSerializer>());
        }
    }
}
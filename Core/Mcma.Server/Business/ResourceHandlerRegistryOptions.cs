using System;
using System.Collections.Generic;
using Mcma.Core.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Server.Business
{
    public class ResourceHandlerRegistryOptions
    {
        /// <summary>
        /// Instantiates a <see cref="ResourceHandlerRegistryOptions"/>
        /// </summary>
        /// <param name="serviceCollection"></param>
        public ResourceHandlerRegistryOptions(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        /// <summary>
        /// Gets the service collection
        /// </summary>
        public IServiceCollection ServiceCollection { get; }

        /// <summary>
        /// Gets the collection of handler type mappings
        /// </summary>
        internal Dictionary<Type, Type> HandlerTypeMappings { get; } = new Dictionary<Type, Type>();
        
        /// <summary>
        /// Gets or sets the custom delegate used to create a handler
        /// </summary>
        public Func<Type, IResourceHandler> CreateHandler { get; set; }

        /// <summary>
        /// Gets or sets the default delegate used to create a handler
        /// </summary>
        public Func<Type, IResourceHandler> DefaultCreateHandler { get; private set; }

        /// <summary>
        /// Registers a resource handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public ResourceHandlerRegistryOptions Register<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where T : Resource, new()
        {
            HandlerTypeMappings[typeof(T)] = typeof(ResourceHandler<T>);
            ServiceCollection.AddScoped(typeof(ResourceHandler<T>));
            return this;
        }

        /// <summary>
        /// Registers a resource handler
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public ResourceHandlerRegistryOptions Register<TResource, THandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TResource : Resource, new()
            where THandler : IResourceHandler
        {
            HandlerTypeMappings[typeof(TResource)] = typeof(THandler);
            ServiceCollection.AddScoped(typeof(THandler));
            return this;
        }

        /// <summary>
        /// Configures the options with the given service provider
        /// </summary>
        /// <returns></returns>
        internal ResourceHandlerRegistryOptions Configure(IServiceProvider svcProvider)
        {
            DefaultCreateHandler =
                t =>
                {
                    var logger = svcProvider.GetService<ILogger>();
                    try
                    {
                        logger.Info("Creating resource handler for resource type {0}...", t.Name);

                        // get the handler type for the provided resource type
                        var handlerType = GetHandlerType(t);

                        logger.Info("Handler type for resource type {0} = {1}", t.Name, handlerType.Name);

                        var handler = (IResourceHandler)svcProvider.GetService(handlerType);
                        if (handler == null)
                            logger.Warning(
                                "Failed to create default handler for resource type {ResourceType}. Service provider was unable to resolve handler type {ResourceHandlerType}.",
                                t.Name,
                                handlerType);

                        return handler;
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred creating resource handler for type {0}. Exception: {1}", t.Name, e);
                        throw;
                    }
                };

            return this;
        }

        /// <summary>
        /// Checks if the provided type is supported
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal bool IsSupported(Type type) => GetHandlerType(type) != null;

        /// <summary>
        /// Gets the handler type for a given resource type
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        private Type GetHandlerType(Type resourceType)
        {
            Type closestMatch = null;

            foreach (var registeredType in HandlerTypeMappings.Keys)
            {
                // if the registered type is a base class of, or an exact match with, the specified resource type, it's a match
                // if another match was already found, but it's a base class of this match, override it with this one
                if (registeredType.IsAssignableFrom(resourceType) && (closestMatch == null || closestMatch.IsAssignableFrom(registeredType)))
                    closestMatch = registeredType;

                // if it's an exact match, we can stop looking
                if (closestMatch == resourceType)
                    break;
            }

            return closestMatch != null ? HandlerTypeMappings[closestMatch] : null;
        }
    }
}
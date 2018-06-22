using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;
using Mcma.Server.Environment;

namespace Mcma.Server.Data
{
    public class ResourceDataHandler : IResourceDataHandler
    {
        /// <summary>
        /// Instantiates a <see cref="ResourceDataHandler"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="environment"></param>
        /// <param name="repositoryHandler"></param>
        /// <param name="httpHandler"></param>
        public ResourceDataHandler(ILogger logger,
                                   IEnvironment environment,
                                   IRepositoryResourceDataHandler repositoryHandler,
                                   IHttpResourceDataHandler httpHandler)
        {
            Logger = logger;
            Environment = environment;
            RepositoryHandler = repositoryHandler;
            HttpHandler = httpHandler;
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the environment
        /// </summary>
        private IEnvironment Environment { get; }

        /// <summary>
        /// Gets the data handler that uses the local service's repository
        /// </summary>
        private IRepositoryResourceDataHandler RepositoryHandler { get; }

        /// <summary>
        /// Gets the data handler that uses HTTP calls
        /// </summary>
        private IHttpResourceDataHandler HttpHandler { get; }

        /// <summary>
        /// Executes an action against either the repository or HTTP handler based on the resource descriptor
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        private Task Execute(ResourceDescriptor resourceDescriptor, Func<IResourceDataHandler, Task> execute)
        {
            return IsLocal(resourceDescriptor) ? execute(RepositoryHandler) : execute(HttpHandler);
        }

        /// <summary>
        /// Executes an action against either the repository or HTTP handler based on the resource descriptor
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        private Task<TResult> Execute<TResult>(ResourceDescriptor resourceDescriptor, Func<IResourceDataHandler, Task<TResult>> execute)
        {
            return IsLocal(resourceDescriptor) ? execute(RepositoryHandler) : execute(HttpHandler);
        }

        /// <summary>
        /// Checks if a resource is local
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        private bool IsLocal(ResourceDescriptor resourceDescriptor)
        {
            var isLocal = !string.IsNullOrWhiteSpace(Environment.PublicUrl()) && resourceDescriptor.Url.StartsWith(Environment.PublicUrl());

            Logger.Debug("Executing operation for {0} resource at {1} (local url = {2})",
                         isLocal ? "local" : "remote",
                         resourceDescriptor.Url,
                         Environment.PublicUrl());

            return isLocal;
        }

        /// <summary>
        /// Gets a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public Task<Resource> Get(ResourceDescriptor resourceDescriptor)
        {
            return Execute(resourceDescriptor, handler => handler.Get(resourceDescriptor));
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public Task<T> Get<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
        {
            return Execute(resourceDescriptor, handler => handler.Get<T>(resourceDescriptor));
        }

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public Task<IEnumerable<T>> Query<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
        {
            Logger.Debug("Querying resources of type {0}...", resourceDescriptor.Type.Name);

            return Execute(resourceDescriptor, handler => handler.Query<T>(resourceDescriptor));
        }

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<Resource> Create(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            return Execute(resourceDescriptor, handler => handler.Create(resourceDescriptor, resource));
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<T> Create<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new()
        {
            return Execute(resourceDescriptor, handler => handler.Create(resourceDescriptor, resource));
        }

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<Resource> Update(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            return Execute(resourceDescriptor, handler => handler.Update(resourceDescriptor, resource));
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<T> Update<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new()
        {
            return Execute(resourceDescriptor, handler => handler.Update(resourceDescriptor, resource));
        }

        /// <summary>
        /// Deletes a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public Task Delete<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
        {
            return Execute(resourceDescriptor, handler => handler.Delete<T>(resourceDescriptor));
        }
    }
}
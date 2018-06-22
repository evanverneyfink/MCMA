using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;
using Mcma.Server.Api;
using Mcma.Server.Data;
using Mcma.Server.Environment;

namespace Mcma.Server.Business
{
    public class ResourceHandler<T> : IResourceHandler, IResourceHandler<T> where T : Resource, new()
    {
        /// <summary>
        /// Instantiates a <see cref="ResourceHandler{T}"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="environment"></param>
        /// <param name="dataHandler"></param>
        /// <param name="resourceDescriptorHelper"></param>
        public ResourceHandler(ILogger logger, IEnvironment environment, IResourceDataHandler dataHandler, IResourceDescriptorHelper resourceDescriptorHelper)
        {
            Logger = logger;
            Environment = environment;
            DataHandler = dataHandler;
            ResourceDescriptorHelper = resourceDescriptorHelper;
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the environment
        /// </summary>
        protected IEnvironment Environment { get; }

        /// <summary>
        /// Gets the repository
        /// </summary>
        protected IResourceDataHandler DataHandler { get; }

        /// <summary>
        /// Gets the resource url helper
        /// </summary>
        protected IResourceDescriptorHelper ResourceDescriptorHelper { get; }

        #region Explicit IResourceHandler Implementation

        /// <summary>
        /// Gets a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        async Task<Resource> IResourceHandler.Get(ResourceDescriptor resourceDescriptor)
        {
            return await Get(resourceDescriptor);
        }

        /// <summary>
        /// Queries resources using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        async Task<IEnumerable<Resource>> IResourceHandler.Query(ResourceDescriptor resourceDescriptor)
        {
            return await Query(resourceDescriptor);
        }

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        async Task<Resource> IResourceHandler.Create(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            return await Create(resourceDescriptor, (T)resource);
        }

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        async Task<Resource> IResourceHandler.Update(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            return await Update(resourceDescriptor, (T)resource);
        }

        /// <summary>
        /// Deletes a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        async Task IResourceHandler.Delete(ResourceDescriptor resourceDescriptor)
        {
            await Delete(resourceDescriptor);
        }

        #endregion

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public virtual async Task<T> Get(ResourceDescriptor resourceDescriptor)
        {
            return await DataHandler.Get<T>(resourceDescriptor);
        }

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> Query(ResourceDescriptor resourceDescriptor)
        {
            return await DataHandler.Query<T>(resourceDescriptor);
        }

        /// <summary>
        /// Creates a new resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<T> Create(ResourceDescriptor resourceDescriptor, T resource)
        {
            return (T)await CreateResource(resourceDescriptor, resource);
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<T> Update(ResourceDescriptor resourceDescriptor, T resource)
        {
            // create any new child resources
            foreach (var childResource in resource.GetChildResources().Where(r => string.IsNullOrWhiteSpace(r.Id)))
                await CreateResource(new ResourceDescriptor(resource.GetType()) { Parent = resourceDescriptor }, childResource);

            // set the modified date to now
            resource.DateModified = DateTime.UtcNow;
            
            // call data layer to update the object
            resource = await DataHandler.Update(resourceDescriptor, resource);

            return resource;
        }

        /// <summary>
        /// Deletes a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public virtual async Task Delete(ResourceDescriptor resourceDescriptor)
        {
            // call data layer to delete the resource
            await DataHandler.Delete<T>(resourceDescriptor);
        }

        /// <summary>
        /// Creates a new resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private async Task<Resource> CreateResource(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            // set new ID and url on the resource descriptor
            resourceDescriptor.Id = Guid.NewGuid().ToString();
            resourceDescriptor.Url =
                (Environment.PublicUrl()?.TrimEnd('/') ?? string.Empty) + ResourceDescriptorHelper.GetUrlPath(resourceDescriptor);

            // set url as the ID of the resource
            resource.Id = resourceDescriptor.Url;

            // set created/modified dates and times
            if (!resource.DateCreated.HasValue)
                resource.DateCreated = DateTime.UtcNow;
            if (!resource.DateModified.HasValue)
                resource.DateModified = DateTime.UtcNow;

            // create child resources
            foreach (var childResource in resource.GetChildResources().Where(r => string.IsNullOrWhiteSpace(r.Id)))
                await CreateResource(new ResourceDescriptor(childResource.GetType()) {Parent = resourceDescriptor}, childResource);

            await DataHandler.Create(resourceDescriptor, resource);

            return resource;
        }
    }
}
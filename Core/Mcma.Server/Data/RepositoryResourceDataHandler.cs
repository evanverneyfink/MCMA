using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;

namespace Mcma.Server.Data
{
    public class RepositoryResourceDataHandler : IRepositoryResourceDataHandler
    {
        /// <summary>
        /// Instantiates a <see cref="RepositoryResourceDataHandler"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="documentHelper"></param>
        public RepositoryResourceDataHandler(IRepository repository, IDocumentHelper documentHelper)
        {
            Repository = repository;
            DocumentHelper = documentHelper;
        }

        /// <summary>
        /// Gets the repository
        /// </summary>
        private IRepository Repository { get; }

        /// <summary>
        /// Gets the document helper
        /// </summary>
        private IDocumentHelper DocumentHelper { get; }

        /// <summary>
        /// Gets a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public async Task<Resource> Get(ResourceDescriptor resourceDescriptor)
        {
            var resource = await Repository.Get(resourceDescriptor.Type, resourceDescriptor.Url);

            return DocumentHelper.GetResource(resourceDescriptor.Type, resource);
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public async Task<T> Get<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
            => (T)await Get(resourceDescriptor);

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> Query<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
        {
            var resources = await Repository.Query<T>(resourceDescriptor.Parameters);

            return resources.Select<dynamic, T>(r => DocumentHelper.GetResource<T>(r));
        }

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<Resource> Create(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            var newResource = await Repository.Create(resourceDescriptor.Type, DocumentHelper.GetDocument(resource));

            return DocumentHelper.GetResource(resourceDescriptor.Type, newResource);
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<T> Create<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new()
            => (T)await Create(resourceDescriptor, (Resource)resource);

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<Resource> Update(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            var newResource = await Repository.Update(resourceDescriptor.Type, DocumentHelper.GetDocument(resource));

            return DocumentHelper.GetResource(resourceDescriptor.Type, newResource);
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<T> Update<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new()
            => (T)await Update(resourceDescriptor, (Resource)resource);

        /// <summary>
        /// Deletes a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public Task Delete<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
            => Repository.Delete(resourceDescriptor.Type, resourceDescriptor.Id);
    }
}
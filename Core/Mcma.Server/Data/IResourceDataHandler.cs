using System.Collections.Generic;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;

namespace Mcma.Server.Data
{
    public interface IResourceDataHandler
    {
        /// <summary>
        /// Gets a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task<Resource> Get(ResourceDescriptor resourceDescriptor);

        /// <summary>
        /// Gets a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task<T> Get<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new();

        /// <summary>
        /// Queries resources using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> Query<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new();

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<Resource> Create(ResourceDescriptor resourceDescriptor, Resource resource);

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<T> Create<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new();

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<Resource> Update(ResourceDescriptor resourceDescriptor, Resource resource);

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<T> Update<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new();

        /// <summary>
        /// Deletes a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task Delete<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new();
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;

namespace Mcma.Server.Business
{
    public interface IResourceHandler
    {
        /// <summary>
        /// Gets a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task<Resource> Get(ResourceDescriptor resourceDescriptor);

        /// <summary>
        /// Queries resources using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task<IEnumerable<Resource>> Query(ResourceDescriptor resourceDescriptor);

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<Resource> Create(ResourceDescriptor resourceDescriptor, Resource resource);

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<Resource> Update(ResourceDescriptor resourceDescriptor, Resource resource);

        /// <summary>
        /// Deletes a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task Delete(ResourceDescriptor resourceDescriptor);
    }

    public interface IResourceHandler<T> where T : Resource
    {
        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task<T> Get(ResourceDescriptor resourceDescriptor);

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> Query(ResourceDescriptor resourceDescriptor);

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<T> Create(ResourceDescriptor resourceDescriptor, T resource);

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<T> Update(ResourceDescriptor resourceDescriptor, T resource);

        /// <summary>
        /// Deletes a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        Task Delete(ResourceDescriptor resourceDescriptor);
    }
}
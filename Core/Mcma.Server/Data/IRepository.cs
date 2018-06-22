using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mcma.Core.Model;

namespace Mcma.Server.Data
{
    public interface IRepository
    {
        /// <summary>
        /// Gets a resource of type <see cref="type"/> by its ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<dynamic> Get(Type type, string id);

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<dynamic> Get<T>(string id) where T : Resource, new();

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> Query<T>(IDictionary<string, string> parameters) where T : Resource, new();

        /// <summary>
        /// Creates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<dynamic> Create<T>(dynamic resource) where T : Resource, new();

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<dynamic> Create(Type type, dynamic resource);

        /// <summary>
        /// Updates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<dynamic> Update<T>(dynamic resource) where T : Resource, new();

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<dynamic> Update(Type type, dynamic resource);

        /// <summary>
        /// Deletes a resource of type by its ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task Delete(Type type, string id);
    }
}

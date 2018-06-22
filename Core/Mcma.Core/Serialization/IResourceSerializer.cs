using System.Collections.Generic;
using System.Threading.Tasks;
using Mcma.Core.Model;

namespace Mcma.Core.Serialization
{
    public interface IResourceSerializer
    {
        /// <summary>
        /// Serializes a resource to text
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="linksOnly"></param>
        /// <returns></returns>
        string Serialize(Resource resource, bool linksOnly = true);

        /// <summary>
        /// Serializes a collection of resources to text
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="linksOnly"></param>
        /// <returns></returns>
        string Serialize(IEnumerable<Resource> resources, bool linksOnly = true);

        /// <summary>
        /// Deserializes a resource from text
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serialized"></param>
        /// <param name="resolveLinks"></param>
        /// <returns></returns>
        Task<T> Deserialize<T>(string serialized, bool resolveLinks = false);

        /// <summary>
        /// Deserializes a resource from text
        /// </summary>
        /// <param name="serialized"></param>
        /// <param name="resolveLinks"></param>
        /// <returns></returns>
        Task<Resource> Deserialize(string serialized, bool resolveLinks = true);
    }
}
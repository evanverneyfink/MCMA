using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;

namespace Mcma.Server.Data
{
    public static class ResourceDataHandlerExtensions
    {
        /// <summary>
        /// Gets a resource by its url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceDataHandler"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Task<T> Get<T>(this IResourceDataHandler resourceDataHandler, string url) where T : Resource, new()
        {
            return resourceDataHandler.Get<T>(ResourceDescriptor.FromUrl<T>(url));
        }

        /// <summary>
        /// Gets a resource by its url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceDataHandler"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Task<Resource> Get(this IResourceDataHandler resourceDataHandler, string url)
        {
            return resourceDataHandler.Get(ResourceDescriptor.FromUrl<Resource>(url));
        }

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceDataHandler"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static Task<T> Update<T>(this IResourceDataHandler resourceDataHandler, T resource) where T : Resource, new()
        {
            return resourceDataHandler.Update(ResourceDescriptor.FromUrl<T>(resource.Id), resource);
        }
    }
}
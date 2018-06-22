using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
using Mcma.Server.Environment;

namespace Mcma.Server.Data
{
    public class HttpResourceDataHandler : IHttpResourceDataHandler
    {
        /// <summary>
        /// Instantiates a <see cref="HttpResourceDataHandler"/>
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="resourceSerializer"></param>
        public HttpResourceDataHandler(IEnvironment environment, IResourceSerializer resourceSerializer)
        {
            Environment = environment;
            ResourceSerializer = resourceSerializer;
        }

        /// <summary>
        /// Gets the environment
        /// </summary>
        private IEnvironment Environment { get; }

        /// <summary>
        /// Gets the resource serializer
        /// </summary>
        private IResourceSerializer ResourceSerializer { get; }

        /// <summary>
        /// Gets the repository
        /// </summary>
        private HttpClient HttpClient { get; } = new HttpClient();

        /// <summary>
        /// Gets a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public async Task<Resource> Get(ResourceDescriptor resourceDescriptor)
        {
            var resp = await HttpClient.GetAsync(resourceDescriptor.Url);

            resp.EnsureSuccessStatusCode();

            return await ResourceSerializer.DeserializeResponseBodyToResource(resourceDescriptor.Type, resp);
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public async Task<T> Get<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
        {
            var resp = await HttpClient.GetAsync(resourceDescriptor.Url);

            resp.EnsureSuccessStatusCode();

            return await ResourceSerializer.DeserializeResponseBodyToResource<T>(resp);
        }

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> Query<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
        {
            var resp = await HttpClient.GetAsync(resourceDescriptor.Url);

            resp.EnsureSuccessStatusCode();

            return await ResourceSerializer.DeserializeResponseBodyToResourceCollection<T>(resp);
        }

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<Resource> Create(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            var resp = await HttpClient.PostAsync(resourceDescriptor.Url, new JsonContent(ResourceSerializer.Serialize(resource)));

            resp.EnsureSuccessStatusCode();

            return await ResourceSerializer.DeserializeResponseBodyToResource(resource.GetType(), resp);
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<T> Create<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new()
        {
            var resp = await HttpClient.PostAsync(resourceDescriptor.Url, new JsonContent(ResourceSerializer.Serialize(resource)));

            resp.EnsureSuccessStatusCode();

            return await ResourceSerializer.DeserializeResponseBodyToResource<T>(resp);
        }

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<Resource> Update(ResourceDescriptor resourceDescriptor, Resource resource)
        {
            var resp = await HttpClient.PutAsync(resourceDescriptor.Url, new JsonContent(ResourceSerializer.Serialize(resource)));

            resp.EnsureSuccessStatusCode();

            return await ResourceSerializer.DeserializeResponseBodyToResource(resource.GetType(), resp);
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<T> Update<T>(ResourceDescriptor resourceDescriptor, T resource) where T : Resource, new()
        {
            var resp = await HttpClient.PutAsync(resourceDescriptor.Url, new JsonContent(ResourceSerializer.Serialize(resource)));

            resp.EnsureSuccessStatusCode();

            return await ResourceSerializer.DeserializeResponseBodyToResource<T>(resp);
        }

        /// <summary>
        /// Deletes a resource by its ID
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public virtual async Task Delete<T>(ResourceDescriptor resourceDescriptor) where T : Resource, new()
        {
            (await HttpClient.DeleteAsync(resourceDescriptor.Url)).EnsureSuccessStatusCode();
        }
    }
}
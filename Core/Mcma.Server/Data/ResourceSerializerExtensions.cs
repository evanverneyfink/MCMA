using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
using Newtonsoft.Json.Linq;

namespace Mcma.Server.Data
{
    public static class ResourceSerializerExtensions
    {
        /// <summary>
        /// Deserializes a response body to a single resource
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="type"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<Resource> DeserializeResponseBodyToResource(this IResourceSerializer serializer, Type type, HttpResponseMessage response)
        {
            return await serializer.Deserialize(await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Deserializes a response body to a single resource
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<T> DeserializeResponseBodyToResource<T>(this IResourceSerializer serializer, HttpResponseMessage response)
            where T : Resource, new()
        {
            return await serializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Deserializes a response body to a collection of resources
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> DeserializeResponseBodyToResourceCollection<T>(this IResourceSerializer serializer, HttpResponseMessage response)
            where T : Resource, new()
        {
            return await Task.WhenAll(JArray.Parse(await response.Content.ReadAsStringAsync()).Select(t => serializer.Deserialize<T>(t.ToString())));
        }
    }
}
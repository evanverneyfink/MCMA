using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Mcma.Core.Model;

namespace Mcma.Core
{
    public static class ExpandoExtensions
    {
        /// <summary>
        /// Converts an <see cref="ExpandoObject"/> into a <see cref="Resource"/>
        /// </summary>
        /// <param name="expando"></param>
        /// <returns></returns>
        public static Resource ToResource(this ExpandoObject expando)
        {
            var dict = new Dictionary<string, object>(expando, StringComparer.OrdinalIgnoreCase);

            if (!dict.ContainsKey(nameof(Resource.Type)))
                throw new Exception($"Provided object cannot be translated to a Resource, as it does not specify a type.");

            var resourceType = dict[nameof(Resource.Type)].ToString().ToResourceType();

            var resource = (Resource)Activator.CreateInstance(resourceType);

            foreach (var prop in resourceType.GetProperties().Where(p => p.CanWrite && dict.ContainsKey(p.Name)))
                prop.SetValue(resource, dict[prop.Name]);

            return resource;
        }

        /// <summary>
        /// Converts an <see cref="ExpandoObject"/> into a <see cref="Resource"/> of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expando"></param>
        /// <returns></returns>
        public static T ToResource<T>(this ExpandoObject expando) where T : Resource => (T)expando.ToResource();
    }
}
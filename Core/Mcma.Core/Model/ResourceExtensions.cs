using System.Collections.Generic;
using System.Linq;

namespace Mcma.Core.Model
{
    public static class ResourceExtensions
    {
        public static IEnumerable<Resource> GetAllResources(this Resource resource)
        {
            var resources = new List<Resource> {resource};

            foreach (var prop in resource.GetType().GetProperties())
            {
                if (typeof(Resource).IsAssignableFrom(prop.PropertyType))
                {
                    if (prop.GetValue(resource) is Resource child)
                        resources.AddRange(child.GetAllResources());
                }
                else if (prop.PropertyType.IsAssignableToEnumerableOf<Resource>())
                {
                    if (prop.GetValue(resource) is IEnumerable<Resource> childCollection)
                        resources.AddRange(childCollection.SelectMany(GetAllResources));
                }
            }

            return resources;
        }

        /// <summary>
        /// Gets child resources for a given resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static IEnumerable<Resource> GetChildResources(this Resource resource)
        {
            var resources = new List<Resource>();

            foreach (var prop in resource.GetType().GetProperties().Where(p => p.PropertyType.IsAssignableToEnumerableOf<Resource>()))
            {
                if (prop.GetValue(resource) is IEnumerable<Resource> childCollection)
                    resources.AddRange(childCollection);
            }

            return resources;
        }
    }
}
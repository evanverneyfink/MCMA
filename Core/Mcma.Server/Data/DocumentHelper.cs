using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Mcma.Core;
using Mcma.Core.Model;

namespace Mcma.Server.Data
{
    internal class DocumentHelper : IDocumentHelper
    {
        /// <summary>
        /// Gets a document from a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public dynamic GetDocument(Resource resource)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var prop in resource.GetType().GetProperties())
            {
                var propValue = prop.GetValue(resource);

                switch (propValue)
                {
                    case Resource linkedResource:
                        expando[prop.Name] = linkedResource.Id;
                        break;
                    case IEnumerable<Resource> linkedResources:
                        expando[prop.Name] = linkedResources.Select(r => r.Id).ToArray();
                        break;
                    case IEnumerable<Type> types:
                        expando[prop.Name] = types.Select(t => t.Name).ToArray();
                        break;
                    case Type type:
                        expando[prop.Name] = type.Name;
                        break;
                    default:
                        expando[prop.Name] = propValue;
                        break;
                }
            }

            return expando;
        }

        /// <summary>
        /// Gets a resource from a document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        public T GetResource<T>(dynamic document) where T : Resource, new() => (T)GetResource(typeof(T), document);

        /// <summary>
        /// Gets a resource from a document
        /// </summary>
        /// <param name="type"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public Resource GetResource(Type type, dynamic document) => (Resource)ConvertDocumentToObject(type, document);

        /// <summary>
        /// Gets the property value provider dictionary
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private IDictionary<string, Func<object>> GetPropertyValueProviderDictionary(dynamic document)
        {
            // if this is already a dictionary (e.g. ExpandoObject or JObject), just return the values
            if (document is IDictionary<string, object> dict)
                return dict.ToDictionary(kvp => kvp.Key, kvp => new Func<object>(() => kvp.Value));
            
            var propertyValues = new Dictionary<string, PropertyInfo>();

            foreach (var prop in ((Type)document.GetType()).GetProperties())
            {
                // This handles cases where the same property exists twice on the object (different classes in the class hierarchy).
                // If the current property's declaring type is a base class to the existing property's declaring type, we want to
                // keep the property we already have. Otherwise, we overwrite it with the property from the derived class.
                if (propertyValues.ContainsKey(prop.Name) &&
                    prop.DeclaringType.IsAssignableFrom(propertyValues[prop.Name].DeclaringType))
                    continue;

                propertyValues[prop.Name] = prop;
            }

            return propertyValues.ToDictionary(kvp => kvp.Key, kvp => new Func<object>(() => kvp.Value.GetValue(document)));
        }

        /// <summary>
        /// Converts a document to an object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        private object ConvertDocumentToObject(Type type, dynamic document)
        {
            var resource = Activator.CreateInstance(type);

            IDictionary<string, Func<object>> docProps = GetPropertyValueProviderDictionary(document);

            foreach (var prop in type.GetProperties())
            {
                if (!docProps.ContainsKey(prop.Name))
                    continue;

                var propValue = docProps[prop.Name]();
                if (propValue == null)
                    continue;

                if (typeof(Resource).IsAssignableFrom(prop.PropertyType))
                {
                    var linkedResource = (Resource)Activator.CreateInstance(prop.PropertyType);
                    linkedResource.Id = (string)propValue;
                    prop.SetValue(resource, linkedResource);
                }
                else if (typeof(IEnumerable<Resource>).IsAssignableFrom(prop.PropertyType))
                {
                    var itemType = prop.PropertyType.GenericTypeArguments[0];

                    var linkedResources = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

                    foreach (var linkedResourceId in ((IEnumerable<object>)propValue).OfType<string>())
                    {
                        var linkedResource = (Resource)Activator.CreateInstance(itemType);
                        linkedResource.Id = linkedResourceId;
                        linkedResources.Add(linkedResource);
                    }

                    prop.SetValue(resource, linkedResources);
                }
                else if (typeof(IEnumerable<Type>).IsAssignableFrom(prop.PropertyType))
                    prop.SetValue(resource, ((IEnumerable<object>)propValue).OfType<string>().Select(s => s.ToResourceType()).ToList());
                else if (typeof(IEnumerable<object>).IsAssignableFrom(prop.PropertyType))
                {
                    var itemType = prop.PropertyType.GenericTypeArguments[0];

                    var childCollection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

                    foreach (var childObj in (IEnumerable<object>)propValue)
                        childCollection.Add(ConvertDocumentToObject(itemType, childObj));

                    prop.SetValue(resource, childCollection);
                }
                else if (prop.PropertyType == typeof(Type))
                    prop.SetValue(resource, propValue.ToString().ToResourceType());
                else
                    prop.SetValue(resource, propValue);
            }

            return resource;
        }
    }
}
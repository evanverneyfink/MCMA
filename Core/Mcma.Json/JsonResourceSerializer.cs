using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Core;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mcma.Json
{
    internal class JsonResourceSerializer : IResourceSerializer
    {
        /// <summary>
        /// Instantiates a <see cref="JsonResourceSerializer"/>
        /// </summary>
        /// <param name="options"></param>
        public JsonResourceSerializer(IOptions<JsonResourceSerializationOptions> options)
        {
            Options = options?.Value ?? new JsonResourceSerializationOptions();
        }

        /// <summary>
        /// Gets the JSON serialization settings
        /// </summary>
        private JsonResourceSerializationOptions Options { get; }

        /// <summary>
        /// Serializes a resource to text
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="linksOnly"></param>
        /// <returns></returns>
        public string Serialize(Resource resource, bool linksOnly = true)
        {
            return GetJsonObject(resource).ToString(Formatting.None);
        }

        /// <summary>
        /// Serializes a collection of resources to text
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="linksOnly"></param>
        /// <returns></returns>
        public string Serialize(IEnumerable<Resource> resources, bool linksOnly = true)
        {
            return new JArray(resources.Select(GetJsonObject)).ToString(Formatting.None);
        }

        /// <summary>
        /// Gets JSON from an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private JObject GetJsonObject(object obj)
        {
            var jObj = JObject.FromObject(obj, JsonSerializer.CreateDefault(Options.JsonSerializerSettings));

            // replace "type" with "@type"
            if (jObj.ContainsKey("type"))
            {
                jObj["@type"] = jObj["type"];
                jObj.Remove("type");
            }

            var props = obj.GetType().GetProperties().ToDictionary(p => p.Name.PascalCaseToCamelCase(), p => p.PropertyType);

            foreach (var jProp in jObj.Properties())
            {
                if (jProp.Value.Type == JTokenType.Object && typeof(Resource).IsAssignableFrom(props[jProp.Name]))
                    jObj[jProp.Name] = jProp.Value.Value<JObject>()["id"];
                else if (jProp.Value.Type == JTokenType.Array && typeof(IEnumerable<Resource>).IsAssignableFrom(props[jProp.Name]))
                    jObj[jProp.Name] = new JArray(jProp.Value.Value<JArray>().OfType<JObject>().Select(i => i["id"]));
            }

            return jObj;
        }

        /// <summary>
        /// Deserializes a resource from text
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serialized"></param>
        /// <param name="resolveLinks"></param>
        /// <returns></returns>
        public Task<T> Deserialize<T>(string serialized, bool resolveLinks = false)
        {
            return Task.FromResult((T)GetObject(JObject.Parse(serialized), typeof(T)));
        }

        /// <summary>
        /// Deserializes a resource from text
        /// </summary>
        /// <param name="serialized"></param>
        /// <param name="resolveLinks"></param>
        /// <returns></returns>
        public Task<Resource> Deserialize(string serialized, bool resolveLinks = true)
        {
            return Task.FromResult((Resource)GetObject(JObject.Parse(serialized)));
        }

        /// <summary>
        /// Gets an object from JSON
        /// </summary>
        /// <param name="jObj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private object GetObject(JObject jObj, Type type = null)
        {
            // try to get type from JSON
            // if the JSON does not specify a type and an explicit type is not set, this will throw an exception
            // otherwise, it will fallback to the specified type
            type = GetTypeFromJson(jObj, type == null) ?? type;

            if (type == null)
                throw new Exception("Unable to determine object type from JSON.");
            
            var props = type.GetProperties().ToDictionary(p => p.Name.PascalCaseToCamelCase(), p => p.PropertyType);

            foreach (var jProp in jObj.Properties())
            {
                if (jProp.Value.Type == JTokenType.String && typeof(Resource).IsAssignableFrom(props[jProp.Name]))
                    jObj[jProp.Name] = new JObject {["id"] = jProp.Value.Value<string>()};
                else if (jProp.Value.Type == JTokenType.Array && typeof(IEnumerable<Resource>).IsAssignableFrom(props[jProp.Name]))
                    jObj[jProp.Name] = new JArray(jProp.Value.Value<JArray>()
                                                       .Select(i => i.Type == JTokenType.String ? new JObject {["id"] = i.Value<string>()} : i));
            }

            return jObj.ToObject(type, JsonSerializer.CreateDefault(Options.JsonSerializerSettings));
        }

        /// <summary>
        /// Gets the resource type for an object from JSON
        /// </summary>
        /// <param name="jObj"></param>
        /// <param name="throwOnFailure"></param>
        /// <returns></returns>
        private Type GetTypeFromJson(JObject jObj, bool throwOnFailure)
        {
            var typeName = jObj["@type"];
            if (typeName == null)
            {
                if (!throwOnFailure) return null;
                throw new Exception(
                    $"Cannot deserialize JSON. Resource type to which to deserialize was not specified in code, and the provided JSON object does not contain a '{nameof(Resource.Type).PascalCaseToCamelCase()}' property.");
            }

            if (typeName.Type != JTokenType.String)
            {
                if (!throwOnFailure) return null;
                throw new Exception(
                    $"Cannot deserialize JSON. The '{nameof(Resource.Type).PascalCaseToCamelCase()}' property in the JSON object is not a string.");
            }

            var type = typeName.Value<string>().ToResourceType();
            if (type == null)
            {
                if (!throwOnFailure) return null;
                throw new Exception(
                    $"Cannot deserialize JSON. The '{nameof(Resource.Type).PascalCaseToCamelCase()}' property specifies type '{typeName.Value<string>()}' which is not a recognized resource type.");
            }

            return type;
        }
    }

    #region Old

    //{
    //    /// <summary>
    //    /// Instantiates a <see cref="JsonResourceSerializer"/>
    //    /// </summary>
    //    /// <param name="options"></param>
    //    public JsonResourceSerializer(IOptions<JsonResourceSerializationOptions> options)
    //    {
    //        Options = options?.Value ?? new JsonResourceSerializationOptions();
    //    }

    //    /// <summary>
    //    /// Gets the JSON serialization settings
    //    /// </summary>
    //    private JsonResourceSerializationOptions Options { get; }

    //    /// <summary>
    //    /// Serializes a resource to JSON
    //    /// </summary>
    //    /// <param name="resource"></param>
    //    /// <param name="linksOnly"></param>
    //    /// <returns></returns>
    //    public string Serialize(Resource resource, bool linksOnly = true)
    //    {
    //        var json = new StringBuilder();

    //        var serializer = JsonSerializer.CreateDefault(Options.JsonSerializerSettings);

    //        using (var writer = new JsonTextWriter(new StringWriter(json)))
    //        {
    //            WriteResource(writer, serializer, resource, linksOnly);

    //            writer.Flush();
    //        }

    //        return json.ToString();
    //    }

    //    /// <summary>
    //    /// Writes a <see cref="Resource"/> as JSON
    //    /// </summary>
    //    /// <param name="writer"></param>
    //    /// <param name="serializer"></param>
    //    /// <param name="resource"></param>
    //    /// <param name="linksOnly"></param>
    //    private void WriteResource(JsonWriter writer, JsonSerializer serializer, Resource resource, bool linksOnly)
    //    {
    //        // write the start of the JSON object
    //        writer.WriteStartObject();

    //        foreach (var prop in resource.GetType().GetProperties())
    //        {
    //            // write the property
    //            writer.WritePropertyName(prop.Name.PascalCaseToCamelCase());

    //            var propValue = prop.GetValue(resource);

    //            if (propValue == null)
    //                writer.WriteNull();
    //            else if (typeof(Resource).IsAssignableFrom(prop.PropertyType))
    //            {
    //                // get the child resource
    //                var child = (Resource)propValue;

    //                // write either the link/ID or the entire object, based on the provided flag
    //                if (linksOnly)
    //                    serializer.Serialize(writer, child.Id);
    //                else
    //                    WriteResource(writer, serializer, child, false);
    //            }
    //            else if (prop.PropertyType.IsAssignableToEnumerableOf<Resource>())
    //            {
    //                //  get child collection
    //                var children = ((IEnumerable)prop.GetValue(resource))?.OfType<Resource>();
    //                if (children == null)
    //                    continue;

    //                // write start of JSON array
    //                writer.WriteStartArray();

    //                // foreach object, we're either going to write the link/ID or the whole object
    //                foreach (var child in children)
    //                {
    //                    if (linksOnly)
    //                        serializer.Serialize(writer, child.Id);
    //                    else
    //                        WriteResource(writer, serializer, child, false);
    //                }

    //                // write end of JSON array
    //                writer.WriteEndArray();
    //            }
    //            else if (prop.PropertyType.IsAssignableToEnumerableOf<Type>())
    //                serializer.Serialize(writer, ((IEnumerable<Type>)propValue).Select(t => t.ToResourceTypeName()).ToArray());
    //            else if (prop.PropertyType == typeof(Type))
    //                serializer.Serialize(writer, ((Type)propValue).ToResourceTypeName());
    //            else
    //                serializer.Serialize(writer, propValue);
    //        }

    //        // write the end of the JSON object
    //        writer.WriteEndObject();
    //    }

    //    /// <summary>
    //    /// Deserializes a resource from JSON
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="serialized"></param>
    //    /// <param name="resolveLinks"></param>
    //    /// <returns></returns>
    //    public async Task<T> Deserialize<T>(string serialized, bool resolveLinks = true) where T : Resource, new()
    //    {
    //        return (T)await Deserialize(serialized, resolveLinks);
    //    }

    //    /// <summary>
    //    /// Deserializes a resource from text
    //    /// </summary>
    //    /// <param name="serialized"></param>
    //    /// <param name="resolveLinks"></param>
    //    /// <returns></returns>
    //    public async Task<Resource> Deserialize(string serialized, bool resolveLinks = true)
    //    {
    //        // parse a JSON object first
    //        var jObj = JObject.Parse(serialized);

    //        // get the type of the object
    //        var type = GetTypeFromJson(jObj);

    //        // convert the object
    //        return await ReadResource(jObj, type, resolveLinks);
    //    }

    //    /// <summary>
    //    /// Gets the resource type for an object from JSON
    //    /// </summary>
    //    /// <param name="jObj"></param>
    //    /// <returns></returns>
    //    private Type GetTypeFromJson(JObject jObj)
    //    {
    //        var typeName = jObj[nameof(Resource.Type).PascalCaseToCamelCase()];
    //        if (typeName == null)
    //            throw new Exception(
    //                $"Cannot deserialize JSON. Resource type to which to deserialize was not specified in code, and the provided JSON object does not contain a '{nameof(Resource.Type).PascalCaseToCamelCase()}' property.");
    //        if (typeName.Type != JTokenType.String)
    //            throw new Exception(
    //                $"Cannot deserialize JSON. The '{nameof(Resource.Type).PascalCaseToCamelCase()}' property in the JSON object is not a string.");

    //        var type = typeName.Value<string>().ToResourceType();
    //        if (type == null)
    //            throw new Exception(
    //                $"Cannot deserialize JSON. The '{nameof(Resource.Type).PascalCaseToCamelCase()}' property specifies type '{typeName.Value<string>()}' which is not a recognized resource type.");

    //        return type;
    //    }

    //    /// <summary>
    //    /// Reads a <see cref="Resource"/> from JSON
    //    /// </summary>
    //    /// <param name="jObj"></param>
    //    /// <param name="type"></param>
    //    /// <param name="resolveLinks"></param>
    //    /// <returns></returns>
    //    public async Task<Resource> ReadResource(JObject jObj, Type type, bool resolveLinks)
    //    {
    //        // ensure the type is a resource type
    //        if (!typeof(Resource).IsAssignableFrom(type))
    //            throw new Exception(
    //                $"Type {type.Name} cannot be deserialized from JSON because it does not inherit from type {typeof(Resource).Name}");

    //        // get the empty constructor (if any)
    //        var ctor = type.GetConstructor(Type.EmptyTypes);
    //        if (ctor == null)
    //            throw new Exception($"Type {type.Name} cannot be deserialized from JSON because it does not have a parameterless constructor.");

    //        // create new instance of resource types
    //        var resource = (Resource)ctor.Invoke(null);
    //        var resourceProperties = type.GetProperties();

    //        foreach (var jsonProp in jObj.Properties())
    //        {
    //            // find the matching property on the object
    //            var prop = resourceProperties.FirstOrDefault(p => p.Name.Equals(jsonProp.Name, StringComparison.OrdinalIgnoreCase));
    //            if (prop == null)
    //                continue;

    //            if (typeof(Resource).IsAssignableFrom(prop.PropertyType))
    //            {
    //                // get child object
    //                JObject childObj;

    //                switch (jsonProp.Value.Type)
    //                {
    //                    case JTokenType.String:
    //                        childObj = new JObject {[nameof(Resource.Id)] = jsonProp.Value.Value<string>()};
    //                        break;
    //                    case JTokenType.Object:
    //                        childObj = (JObject)jsonProp.Value;
    //                        break;
    //                    case JTokenType.Null:
    //                        childObj = null;
    //                        break;
    //                    default:
    //                        throw new Exception(
    //                            $"Cannot convert JSON token of type {jsonProp.Value.Type} to resource type {prop.PropertyType.Name}.");
    //                }

    //                prop.SetValue(resource, childObj != null ? await ReadResource(childObj, prop.PropertyType, resolveLinks) : null);
    //            }
    //            else if (prop.PropertyType.IsAssignableToEnumerableOf<Resource>())
    //            {
    //                var childType = prop.PropertyType.GenericTypeArguments[0];

    //                // create list of child objects
    //                var childCollection =
    //                    (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));

    //                if (jsonProp.Value.Type != JTokenType.Null)
    //                {
    //                    // get child object
    //                    var jArray = jsonProp.Value.Type == JTokenType.Array
    //                                     ? new JArray(((JArray)jsonProp.Value).Select(GetOrWrap))
    //                                     : new JArray {GetOrWrap(jsonProp.Value)};

    //                    // create resource from each object in the array
    //                    foreach (var childJObj in jArray.OfType<JObject>())
    //                        childCollection.Add(await ReadResource(childJObj, childType, resolveLinks));
    //                }

    //                // set the child collection
    //                prop.SetValue(resource, childCollection);
    //            }
    //            else if (prop.PropertyType.IsGenericList())
    //            {
    //                var childType = prop.PropertyType.GetGenericEnumerableItemType();

    //                // create child list
    //                var childCollection =
    //                    (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));

    //                if (jsonProp.Value.Type != JTokenType.Null)
    //                {
    //                    // get child object
    //                    var jArray = jsonProp.Value.Type == JTokenType.Array ? (JArray)jsonProp.Value : new JArray {jsonProp.Value};

    //                    // deserialize and add children to collection
    //                    foreach (var childItem in jArray)
    //                    {
    //                        if (childType == typeof(Type))
    //                            childCollection.Add(childItem.Value<string>().ToResourceType());
    //                        else if (!childType.IsValueType && childType != typeof(string))
    //                            childCollection.Add(childItem.Value<JObject>().ToObject(childType));
    //                        else
    //                            childItem.Value<string>().Parse(childType);
    //                    }
    //                }

    //                // set the child collection on the resource
    //                prop.SetValue(resource, childCollection);
    //            }
    //            else if (prop.PropertyType == typeof(Type))
    //            {
    //                prop.SetValue(resource, jsonProp.Value.Value<string>().ToResourceType());
    //            }
    //            else if (!prop.PropertyType.IsValueType && prop.PropertyType != typeof(string))
    //            {
    //                if (jsonProp.Value.Type == JTokenType.Null)
    //                    prop.SetValue(resource, null);
    //                else if (jsonProp.Value.Type == JTokenType.Object)
    //                    prop.SetValue(resource, ((JObject)jsonProp.Value).ToObject(prop.PropertyType));
    //                else
    //                    throw new Exception(
    //                        $"Invalid JSON. Cannot deserialize JSON token of type {jsonProp.Value.Type} to an object of type {prop.PropertyType.Name}");

    //            }
    //            else
    //            {
    //                prop.SetValue(resource, jsonProp.Value.ToObject(prop.PropertyType));
    //            }
    //        }

    //        return resource;
    //    }

    //    private JObject GetOrWrap(JToken jToken)
    //    {
    //        if (jToken.Type == JTokenType.Null)
    //            return null;

    //        return jToken.Type == JTokenType.String
    //            ? new JObject {[nameof(Resource.Id)] = jToken.Value<string>()}
    //            : (JObject)jToken;
    //    }
    //}

    #endregion
}

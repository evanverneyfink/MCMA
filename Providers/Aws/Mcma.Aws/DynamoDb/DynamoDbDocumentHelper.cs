using System.Collections.Generic;
using System.Dynamic;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Mcma.Aws.DynamoDb
{
    public static class DynamoDbDocumentHelper
    {
        private static JsonConverter ExpandoObjectConverter { get; } = new ExpandoObjectConverter();

        /// <summary>
        /// Converts a <see cref="Document"/> to an object
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static dynamic ToObject(Document document)
        {
            var docJson = JObject.Parse(document.ToJson());

            var resourceJson = docJson[DynamoDbDefaults.ResourceAttribute];

            return resourceJson != null ? JsonConvert.DeserializeObject<ExpandoObject>(resourceJson.ToString(), ExpandoObjectConverter) : null;
        }

        /// <summary>
        /// Converts an object to a <see cref="Document"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static Document ToDocument(dynamic resource)
        {
            return Document.FromJson(
                new JObject
                {
                    [DynamoDbDefaults.ResourceTypeAttribute] = resource.Type,
                    [DynamoDbDefaults.ResourceIdAttribute] = resource.Id,
                    [DynamoDbDefaults.ResourceAttribute] = JObject.FromObject(resource)
                }.ToString());
        }

        /// <summary>
        /// Converts a dictionary of parameters to a <see cref="ScanFilter"/>
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static QueryFilter ToQueryFilter(this IDictionary<string, string> filters)
        {
            var scanFilter = new QueryFilter();

            if (filters != null)
                foreach (var kvp in filters)
                    scanFilter.AddCondition(kvp.Key, ScanOperator.Equal, new Primitive(kvp.Value));

            return scanFilter;
        }
    }
}
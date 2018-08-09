using System;
using System.Collections.Generic;
using System.Dynamic;
using Mcma.Core;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public static class DynamicTableEntityExtensions
    {
        public static dynamic ToExpandoObject(this DynamicTableEntity entity) => entity?.Properties.ToExpandoObject();

        public static dynamic ToExpandoObject(this IDictionary<string, EntityProperty> properties)
        {
            IDictionary<string, object> expandoAsDict = new ExpandoObject();

            foreach (var prop in properties)
            {
                // use . delimiter to unflatten properties
                var propParts = prop.Key.SplitOn(".");
                var cur = expandoAsDict;

                for (var i = 0; i < propParts.Length; i++)
                {
                    // get the current part of the property path
                    var curPart = propParts[i];

                    if (i < propParts.Length - 1)
                    {
                        // ensure the object property is set
                        if (!cur.ContainsKey(curPart))
                            cur[curPart] = new ExpandoObject();

                        // change the current object
                        cur = (IDictionary<string, object>)cur[curPart];
                    }
                    else
                        cur[curPart] = prop.Value.PropertyAsObject;
                }
            }

            return expandoAsDict;
        }

        public static IDictionary<string, EntityProperty> ToEntityProperties(this ExpandoObject entity)
        {
            var dict = new Dictionary<string, EntityProperty>();

            AddProperties(dict, entity);

            return dict;
        }

        private static void AddProperties(IDictionary<string, EntityProperty> properties, ExpandoObject expando, string prefix = "")
        {
            foreach (var kvp in expando)
            {
                var key = prefix + kvp.Key;

                if (kvp.Value is ExpandoObject child)
                    AddProperties(properties, child, key + ".");
                else if (kvp.Value is ExpandoObject[] childCollection)
                    foreach (var childItem in childCollection)
                        AddProperties(properties, childItem, key + ".");
                else
                    properties[key] = EntityProperty.CreateEntityPropertyFromObject(kvp.Value);
            }
        }
    }
}
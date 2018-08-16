using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Mcma.Core;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public static class DynamicTableEntityExtensions
    {
        private const string PropertyDelimiter = "__";

        private const string OpenArrayIndexer = "i_";

        private const string CloseArrayIndexer = "_i";

        public static dynamic ToExpandoObject(this DynamicTableEntity entity) => entity?.Properties.ToExpandoObject();

        public static dynamic ToExpandoObject(this IDictionary<string, EntityProperty> properties)
        {
            IDictionary<string, object> expandoAsDict = new ExpandoObject();

            foreach (var prop in properties)
            {
                // use . delimiter to unflatten properties
                var propParts = prop.Key.SplitOn(PropertyDelimiter);
                var cur = expandoAsDict;

                for (var i = 0; i < propParts.Length; i++)
                {
                    // get the current part of the property path
                    var curPart = propParts[i];

                    var startIndexer = curPart.IndexOf(OpenArrayIndexer, StringComparison.Ordinal);
                    var endIndexer = curPart.IndexOf(CloseArrayIndexer, StringComparison.Ordinal);
                    if (startIndexer > 0 && endIndexer > 0)
                    {
                        if (int.TryParse(curPart.Substring(startIndexer + OpenArrayIndexer.Length, endIndexer - startIndexer - CloseArrayIndexer.Length), out var index))
                        {
                            // get the property name without the indexer
                            curPart = curPart.Substring(0, startIndexer);

                            // if this is the part of the path, this is an array of primitives
                            if (i < propParts.Length - 1)
                            {
                                // ensure the property is a list of expandos
                                if (!cur.ContainsKey(curPart))
                                    cur[curPart] = new List<ExpandoObject>();

                                // get the list of expandos
                                var collection = (List<ExpandoObject>)cur[curPart];

                                // ensure the capacity of the list is enough to put this object at the proper index
                                while (index >= collection.Count)
                                    collection.Add(null);

                                // ensure the object has been created at its index in the collection
                                if (collection[index] == null)
                                    collection[index] = new ExpandoObject();

                                // set the object as the current
                                cur = collection[index];
                            }
                            else
                            {
                                // ensure the property is a list of objects
                                if (!cur.ContainsKey(curPart))
                                    cur[curPart] = new List<object>();

                                // get the list of objects
                                var collection = (List<object>)cur[curPart];

                                // ensure the capacity of the list is enough to put this object at the proper index
                                while (index >= collection.Count)
                                    collection.Add(null);

                                // set the object in the collection
                                collection[index] = prop.Value.PropertyAsObject;
                            }
                        }
                    }
                    else if (i < propParts.Length - 1)
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

                switch (kvp.Value)
                {
                    case ExpandoObject child:
                        AddProperties(properties, child, key + PropertyDelimiter);
                        break;
                    case IEnumerable<object> objCollection:
                        var objArray = objCollection.ToArray();
                        for (var i = 0; i < objArray.Length; i++)
                        {
                            if (objArray[i] is ExpandoObject expandoItem)
                                AddProperties(properties, expandoItem, key + $"{OpenArrayIndexer}{i}{CloseArrayIndexer}{PropertyDelimiter}");
                            else
                                properties[$"{key}{OpenArrayIndexer}{i}{CloseArrayIndexer}"] = EntityProperty.CreateEntityPropertyFromObject(objArray[i]);
                        }
                        break;
                    default:
                        properties[key] = EntityProperty.CreateEntityPropertyFromObject(kvp.Value);
                        break;
                }
            }
        }
    }
}
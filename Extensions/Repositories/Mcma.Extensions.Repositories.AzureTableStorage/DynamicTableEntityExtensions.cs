using System.Collections.Generic;
using System.Dynamic;
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
                expandoAsDict[prop.Key] = prop.Value.PropertyAsObject;

            return expandoAsDict;
        }
    }
}
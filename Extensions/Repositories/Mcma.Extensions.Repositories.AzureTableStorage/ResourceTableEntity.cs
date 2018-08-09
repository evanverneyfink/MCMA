using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public class ResourceTableEntity : ITableEntity
    {
        public static string IdToRowKey(string id) => id?.Replace(":", "_").Replace("/", "|");

        /// <summary>
        /// Instantiates a <see cref="ResourceTableEntity"/>
        /// </summary>
        /// <param name="resourceTypeName"></param>
        /// <param name="resourceId"></param>
        /// <param name="timestamp"></param>
        /// <param name="properties"></param>
        /// <param name="etag"></param>
        public ResourceTableEntity(string resourceTypeName,
                                   string resourceId,
                                   DateTimeOffset? timestamp = null,
                                   IDictionary<string, EntityProperty> properties = null,
                                   string etag = null)
        {
            PartitionKey = resourceTypeName;
            RowKey = IdToRowKey(resourceId);

            if (timestamp.HasValue)
                Timestamp = timestamp.Value;
            if (etag != null)
                ETag = etag;
            if (properties != null)
                ReadEntity(properties, null);
        }

        /// <summary>
        /// Gets or sets the partiton key
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the etag
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Getsr or sets the underlying dynamic object
        /// </summary>
        public ExpandoObject Resource { get; set; }

        /// <summary>
        /// Reads in the entity's propreties
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="operationContext"></param>
        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
            => Resource = properties.ToExpandoObject();

        /// <summary>
        /// Writes the entities properties
        /// </summary>
        /// <param name="operationContext"></param>
        /// <returns></returns>
        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
            => Resource.ToEntityProperties();
    }
}
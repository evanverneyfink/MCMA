using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Azure.TableStorage
{
    public static class TableConfigProviderExtensions
    {
        public static ResourceTableEntity GetResourceTableEntity(this IAzureStorageTableConfigProvider tableConfigProvider,
                                                                 string tableName,
                                                                 string partitionKey,
                                                                 string rowKey,
                                                                 DateTimeOffset? timestamp = null,
                                                                 IDictionary<string, EntityProperty> properties = null,
                                                                 string etag = null)
        {
            var entity = new ResourceTableEntity(tableConfigProvider.GetPartitionKeyFieldName(tableName),
                                                 tableConfigProvider.GetRowKeyFieldName(tableName),
                                                 tableConfigProvider.GetResourceFieldName(tableName))
            {
                PartitionKey = partitionKey,
                RowKey = rowKey
            };


            if (timestamp.HasValue)
                entity.Timestamp = timestamp.Value;
            if (etag != null)
                entity.ETag = etag;
            if (properties != null)
                entity.ReadEntity(properties, null);

            return entity;
        }
    }
}
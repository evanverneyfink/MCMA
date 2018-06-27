using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public class ResourceTableEntity : ITableEntity
    {
        /// <summary>
        /// Instantiates a <see cref="ResourceTableEntity"/>
        /// </summary>
        /// <param name="partitionKeyFieldName"></param>
        /// <param name="rowKeyFieldName"></param>
        /// <param name="resourceFieldName"></param>
        public ResourceTableEntity(string partitionKeyFieldName, string rowKeyFieldName, string resourceFieldName)
        {
            PartitionKeyFieldName = partitionKeyFieldName;
            RowKeyFieldName = rowKeyFieldName;
            ResourceFieldName = resourceFieldName;
        }

        /// <summary>
        /// Gets the name of the partition key field
        /// </summary>
        private string PartitionKeyFieldName { get; }

        /// <summary>
        /// Gets the name of the row key field
        /// </summary>
        private string RowKeyFieldName { get; }

        /// <summary>
        /// Gets the name of resource field
        /// </summary>
        private string ResourceFieldName { get; }

        /// <summary>
        /// Getsr or sets the underlying dynamic object
        /// </summary>
        internal ExpandoObject Resource { get; set; }

        /// <summary>
        /// Gets the underlying dynamic object as a dictionary of properties
        /// </summary>
        private IDictionary<string, object> Properties => Resource;

        /// <summary>
        /// Gets or sets the partiton key
        /// </summary>
        public string PartitionKey
        {
            get => GetProperty<string>(PartitionKeyFieldName);
            set => SetProperty(PartitionKeyFieldName, value);
        }

        /// <summary>
        /// Gets or sets the row key
        /// </summary>
        public string RowKey
        {
            get => GetProperty<string>(RowKeyFieldName);
            set => SetProperty(RowKeyFieldName, value);
        }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get => GetProperty<DateTimeOffset>(nameof(Timestamp));
            set => SetProperty(nameof(Timestamp), value);
        }

        /// <summary>
        /// Gets or sets the etag
        /// </summary>
        public string ETag
        {
            get => GetProperty<string>(nameof(ETag));
            set => SetProperty(nameof(ETag), value);
        }

        /// <summary>
        /// Gets or sets the resource document
        /// </summary>
        public dynamic ResourceDocument
        {
            get => GetProperty<dynamic>(ResourceFieldName);
            set => SetProperty(ResourceFieldName, value);
        }

        /// <summary>
        /// Gets a property's value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        private T GetProperty<T>(string name) => Properties.ContainsKey(name) ? (T)Properties[name] : default(T);

        /// <summary>
        /// Sets a property's value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void SetProperty<T>(string name, T value) => Properties[name] = value;

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
            => Resource.ToDictionary(kvp => kvp.Key, kvp => EntityProperty.CreateEntityPropertyFromObject(kvp.Value));
    }
}
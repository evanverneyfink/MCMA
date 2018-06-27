using System;
using Mcma.Server.Environment;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public class DefaultAzureStorageTableConfigProvider : IAzureStorageTableConfigProvider
    {
        /// <summary>
        /// Instantiates a <see cref="DefaultAzureStorageTableConfigProvider"/>
        /// </summary>
        /// <param name="environment"></param>
        public DefaultAzureStorageTableConfigProvider(IEnvironment environment)
        {
            Environment = environment;
        }

        /// <summary>
        /// Gets the environment
        /// </summary>
        private IEnvironment Environment { get; }

        /// <summary>
        /// Gets flag indicating if tables should be created if they don't exist
        /// </summary>
        public bool CreateIfNotExists => false;

        /// <summary>
        /// Gets the table name for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTableName(Type type) => Environment.TableName();

        /// <summary>
        /// Gets the table name for a type
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public string GetTableName(string typeName) => Environment.TableName();

        /// <summary>
        /// Gets the name of the hash key for a table of the specified type
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetPartitionKeyFieldName(string tableName) => AzureTableStorageDefaults.ResourceTypeProperty;

        /// <summary>
        /// Gets the name of the row key for a table of the specified type
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetRowKeyFieldName(string tableName) => AzureTableStorageDefaults.ResourceIdProperty;

        /// <summary>
        /// Gets the name of the row key for a table of the specified type
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetResourceFieldName(string tableName) => AzureTableStorageDefaults.ResourceProperty;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Server;
using Mcma.Server.Data;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public class TableStorageRepository : IRepository
    {
        /// <summary>
        /// Instantiates a <see cref="TableStorageRepository"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tableConfigProvider"></param>
        /// <param name="options"></param>
        public TableStorageRepository(ILogger logger, IAzureStorageTableConfigProvider tableConfigProvider, IOptions<TableStorageOptions> options)
        {
            Logger = logger;
            TableConfigProvider = tableConfigProvider;
            TableClient = (options?.Value ?? new TableStorageOptions()).CreateTableClient();
        }

        /// <summary>
        /// Gets the Azure Table Storage client
        /// </summary>
        private CloudTableClient TableClient { get; }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the table config provider
        /// </summary>
        private IAzureStorageTableConfigProvider TableConfigProvider { get; }

        /// <summary>
        /// Gets the Azure Table Storage table
        /// </summary>
        private async Task<CloudTable> Table(Type type)
        {
            return await TableWithName(TableConfigProvider.GetTableName(type));
        }
        
        /// <summary>
        /// Gets the Azure Table Storage table
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private async Task<CloudTable> Table(string typeName)
        {
            return await TableWithName(TableConfigProvider.GetTableName(typeName));
        }

        /// <summary>
        /// Gets the Azure Table Storage table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private async Task<CloudTable> TableWithName(string tableName)
        {
            Logger.Debug("Checking if table '{0}' exists in Azure Table Storage...", tableName);

            try
            {
                var cloudTable = TableClient.GetTableReference(tableName);
                if (!await cloudTable.ExistsAsync())
                {
                    if (!TableConfigProvider.CreateIfNotExists)
                        throw new Exception($"Table {tableName} does not exist in Azure Table Storage, and the app is not configured to create it if it does not exist.");

                    Logger.Info("Table '{0}' does not exist in Azure Table Storage. Creating it now...", tableName);

                    await cloudTable.CreateAsync();
                }

                return cloudTable;
            }
            catch (Exception exception)
            {
                Logger.Error("An error occurred loading the Azure Table Storage table for type {0}.\r\nException: {1}.", tableName, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the base query for a given type
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private string TypeCondition(string tableName, string typeName)
        {
            Logger.Debug("Getting type condition for table {0} and type {1}...", tableName, typeName);
            var condition = TableQuery.GenerateFilterCondition(TableConfigProvider.GetPartitionKeyFieldName(tableName), QueryComparisons.Equal, typeName);
            Logger.Debug("Type condition for table {0} and type {1}: {2}", tableName, typeName, condition);
            return condition;
        }

        /// <summary>
        /// Gets a resource by its type and ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dynamic> Get(Type type, string id) => await Get(await Table(type), type.Name, id);

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dynamic> Get<T>(string id) where T : Resource, new() => await Get(typeof(T), id);

        /// <summary>
        /// Gets a resource of a given type and with the provided id from a given table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="typeName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<dynamic> Get(CloudTable table, string typeName, string id)
            => await table.ExecuteRetrieveAsync(TableConfigProvider, typeName, ResourceTableEntity.IdToRowKey(id));

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> Query<T>(IDictionary<string, string> parameters) where T : Resource, new()
        {
            var table = await Table(typeof(T));

            parameters = parameters ?? new Dictionary<string, string>();

            var query = new TableQuery().Where(
                parameters.Aggregate(
                    TypeCondition(table.Name, typeof(T).Name),
                    (cur, kvp) => TableQuery.CombineFilters(cur, "AND", TableQuery.GenerateFilterCondition(kvp.Key, QueryComparisons.Equal, kvp.Value))));

            return await table.ExecuteQueryAsync(TableConfigProvider, query);
        }

        /// <summary>
        /// Creates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Create<T>(dynamic resource) where T : Resource, new() => CreateOrUpdate<T>(resource);

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Create(Type type, dynamic resource) => CreateOrUpdate(type, resource);

        /// <summary>
        /// Updates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Update<T>(dynamic resource) where T : Resource, new() => CreateOrUpdate<T>(resource);

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Update(Type type, dynamic resource) => CreateOrUpdate(type, resource);

        /// <summary>
        /// Deletes a resource of type by its ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Delete(Type type, string id)
        {
            var table = await Table(type);
            await table.ExecuteAsync(TableOperation.Delete(new ResourceTableEntity(type.Name, id)));
        }

        /// <summary>
        /// Creates or updates record in Azure Table Storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        private async Task<T> CreateOrUpdate<T>(dynamic resource) where T : Resource, new()
        {
            var table = await Table(typeof(T));

            var entity = new ResourceTableEntity((string)resource.Type, (string)resource.Id) {Resource = resource};

            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));

            return await Get<T>(resource.Id);
        }

        /// <summary>
        /// Creates or updates record in Azure Table Storage
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private async Task<dynamic> CreateOrUpdate(Type type, dynamic resource)
        {
            var table = await Table((string)resource.Type);

            var entity = new ResourceTableEntity(type.Name, (string)resource.Id) {Resource = resource};

            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));

            return await Get(type, resource.Id);
        }
    }
}

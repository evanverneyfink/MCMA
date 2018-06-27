using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Mcma.Core.Model;
using Mcma.Server;
using Mcma.Server.Data;
using Microsoft.Extensions.Options;
using DynamoDbTable = Amazon.DynamoDBv2.DocumentModel.Table;

namespace Mcma.Extensions.Repositories.DynamoDb
{
    public class DynamoDbRepository : IRepository
    {
        /// <summary>
        /// Instantiates a <see cref="DynamoDbRepository"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tableConfigProvider"></param>
        /// <param name="options"></param>
        public DynamoDbRepository(ILogger logger,
                                  IDynamoDbTableConfigProvider tableConfigProvider,
                                  IOptions<DynamoDbOptions> options)
        {
            Logger = logger;
            TableConfigProvider = tableConfigProvider;

            // create client using credentials, if provided
            var region = options.Value?.RegionEndpoint;
            var creds = options.Value?.Credentials;
            DynamoDb = creds != null ? new AmazonDynamoDBClient(creds, region) : new AmazonDynamoDBClient();
        }

        /// <summary>
        /// Gets the DynamoDB client
        /// </summary>
        private IAmazonDynamoDB DynamoDb { get; }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the DynamoDB table config provider
        /// </summary>
        private IDynamoDbTableConfigProvider TableConfigProvider { get; }

        /// <summary>
        /// Gets the DynamoDB table
        /// </summary>
        private async Task<DynamoDbTable> Table(Type type)
        {
            return await TableWithName(TableConfigProvider.GetTableName(type));
        }
        
        /// <summary>
        /// Gets the DynamoDB table
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private async Task<DynamoDbTable> Table(string typeName)
        {
            return await TableWithName(TableConfigProvider.GetTableName(typeName));
        }

        /// <summary>
        /// Gets the DynamoDB table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private async Task<DynamoDbTable> TableWithName(string tableName)
        {
            var hashKeyName = TableConfigProvider.GetTableHashKeyName(tableName);
            var rangeKeyName = TableConfigProvider.GetTableRangeKeyName(tableName);

            Logger.Debug("Checking if table '{0}' exists in DynamoDB...", tableName);

            try
            {
                if (!DynamoDbTable.TryLoadTable(DynamoDb, tableName, out var table))
                {
                    // if we failed to load the table, this is basically a retry that will throw an exception.
                    // The expectation is that this will expose whatever error caused the TryLoadTable method
                    // to return false, but if for some reason it happens to succed on retry, that also works.
                    if (!TableConfigProvider.CreateIfNotExists)
                        return DynamoDbTable.LoadTable(DynamoDb, tableName);
                        //throw new Exception($"Table {tableName} does not exist in DynamoDB.");

                    Logger.Info("Table '{0}' does not exist in DynamoDB. Creating it now...", tableName);

                    var createResp =
                        await DynamoDb.CreateTableAsync(tableName,
                                                        new List<KeySchemaElement>
                                                        {
                                                            new KeySchemaElement
                                                            {
                                                                AttributeName = hashKeyName,
                                                                KeyType = KeyType.HASH
                                                            },
                                                            new KeySchemaElement
                                                            {
                                                                AttributeName = rangeKeyName,
                                                                KeyType = KeyType.RANGE
                                                            }
                                                        },
                                                        new List<AttributeDefinition>
                                                        {
                                                            new AttributeDefinition
                                                            {
                                                                AttributeName = hashKeyName,
                                                                AttributeType = ScalarAttributeType.S
                                                            },
                                                            new AttributeDefinition
                                                            {
                                                                AttributeName = rangeKeyName,
                                                                AttributeType = ScalarAttributeType.S
                                                            }
                                                        },
                                                        new ProvisionedThroughput(1, 1));

                    if (createResp.HttpStatusCode != HttpStatusCode.OK)
                        throw new Exception($"Failed to create table '{tableName}' in DynamoDB. Response code is {createResp.HttpStatusCode}.");

                    Logger.Info("Successfully created DynamoDB table '{0}'.", tableName);

                    table = DynamoDbTable.LoadTable(DynamoDb, tableName);
                }

                return table;
            }
            catch (Exception exception)
            {
                Logger.Error($"An error occurred loading the DynamoDB table for type {tableName}.", exception);
                throw;
            }
        }

        /// <summary>
        /// Gets a resource by its type and ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dynamic> Get(Type type, string id) => await Get(await Table(type), type.Name, id);

        /// <summary>
        /// Gets a resource by its type and ID
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<dynamic> Get(string typeName, string id) => await Get(await Table(typeName), typeName, id);

        /// <summary>
        /// Gets a resource of a given type and with the provided id from a given table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="typeName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<dynamic> Get(DynamoDbTable table, string typeName, string id)
        {
            var hashKey = new Primitive($"{typeName}");
            var rangeKey = new Primitive(id);

            Logger.Debug("Getting item with hash key {0} and range key {1} from table {2}...", hashKey, rangeKey, table.TableName);

            var result = await table.GetItemAsync(hashKey, rangeKey);

            return result != null ? DynamoDbDocumentHelper.ToObject(result) : null;
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dynamic> Get<T>(string id) where T : Resource, new()
        {
            return await Get(typeof(T), id);
        }

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> Query<T>(IDictionary<string, string> parameters) where T : Resource, new()
        {
            return
                (await (await Table(typeof(T))).Query(new Primitive(typeof(T).Name), parameters.ToQueryFilter()).GetRemainingAsync())
                .Select(d => DynamoDbDocumentHelper.ToObject(d));
        }

        /// <summary>
        /// Creates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Create<T>(dynamic resource) where T : Resource, new()
        {
            return CreateOrUpdate<T>(resource);
        }

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Create(Type type, dynamic resource)
        {
            return CreateOrUpdate(resource);
        }

        /// <summary>
        /// Updates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Update<T>(dynamic resource) where T : Resource, new()
        {
            return CreateOrUpdate<T>(resource);
        }

        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Update(Type type, dynamic resource)
        {
            return CreateOrUpdate(resource);
        }

        /// <summary>
        /// Deletes a resource of type by its ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Delete(Type type, string id)
        {
            await (await Table(type)).DeleteItemAsync(new Primitive(id));
        }

        /// <summary>
        /// Creates or updates record in DynamoDB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        private async Task<T> CreateOrUpdate<T>(dynamic resource) where T : Resource, new()
        {
            await (await Table(typeof(T))).PutItemAsync(DynamoDbDocumentHelper.ToDocument(resource));

            return await Get<T>(resource.Id);
        }

        /// <summary>
        /// Creates or updates record in DynamoDB
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private async Task<dynamic> CreateOrUpdate(dynamic resource)
        {
            await (await Table(resource.Type)).PutItemAsync(DynamoDbDocumentHelper.ToDocument(resource));
            
            return await Get(resource.Type, resource.Id);
        }
    }
}

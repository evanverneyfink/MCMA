using Mcma.Server.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Core.Model;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = LiteDB.JsonSerializer;

namespace Mcma.Server.LiteDb
{
    public class LiteDbRepository : IRepository, IDisposable
    {
        /// <summary>
        /// Gets the LiteDB database
        /// </summary>
        private LiteDatabase Database { get; } = new LiteDatabase("mcma.db");

        /// <summary>
        /// Gets a collection for a given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private LiteCollection<T> GetCollection<T>() where T : Resource, new()
        {
            var coll = Database.GetCollection<T>();
            coll.EnsureIndex(r => r.Id);
            return coll;
        }

        /// <summary>
        /// Gets a collection for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private LiteCollection<BsonDocument> GetCollection(Type type)
        {
            var coll = Database.GetCollection(type.Name);
            coll.EnsureIndex(nameof(Resource.Id));
            return coll;
        }

        /// <summary>
        /// Maps a BSON document to a resource type
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private dynamic MapResource(BsonDocument doc)
        {
            var jObj = JObject.Parse(JsonSerializer.Serialize(doc));
            jObj.Remove("_id");
            return JsonConvert.DeserializeObject<ExpandoObject>(jObj.ToString());
        }

        /// <summary>
        /// Maps a BSON document to a resource type
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private BsonDocument MapDocument(dynamic resource)
        {
            dynamic jObj = JObject.Parse(JsonConvert.SerializeObject(resource));
            jObj._id = resource.Id;
            return JsonSerializer.Deserialize(jObj.ToString());
        }

        /// <summary>
        /// Gets a resource of type <see cref="type"/> by its ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<dynamic> Get(Type type, string id)
        {
            return Task.FromResult<object>(MapResource(GetCollection(type).FindById(id)));
        }

        /// <summary>
        /// Gets a resource of type <see cref="T"/> by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<dynamic> Get<T>(string id) where T : Resource, new()
        {
            return Get(typeof(T), id);
        }

        /// <summary>
        /// Queries resources of type <see cref="T"/> using the provided criteria, in the form of key/value pairs
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<IEnumerable<dynamic>> Query<T>(IDictionary<string, string> parameters) where T : Resource, new()
        {
            var query = parameters != null && parameters.Count > 0
                            ? LiteDB.Query.And(parameters.Select(kvp => LiteDB.Query.EQ(kvp.Key, kvp.Value)).ToArray())
                            : null;

            var collection = GetCollection(typeof(T));

            return Task.FromResult(
                (query != null ? collection.Find(query) : collection.FindAll()).Select<BsonDocument, object>(MapResource));
        }

        /// <summary>
        /// Creates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Create<T>(dynamic resource) where T : Resource, new()
        {
            return Create(typeof(T), resource);
        }

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Create(Type type, dynamic resource)
        {
            BsonDocument doc = MapDocument(resource);
            GetCollection(type).Insert(doc);
            return Task.FromResult<object>(resource);
        }

        /// <summary>
        /// Updates a resource of type <see cref="T"/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Update<T>(dynamic resource) where T : Resource, new()
        {
            return Update(typeof(T), resource);
        }

        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public Task<dynamic> Update(Type type, dynamic resource)
        {
            BsonDocument doc = MapDocument(resource);
            GetCollection(type).Update(doc);
            return Task.FromResult<object>(resource);
        }

        /// <summary>
        /// Deletes a resource of type by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task Delete(Type type, string id)
        {
            GetCollection(type).Delete(id);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes of the underlying database connection
        /// </summary>
        public void Dispose()
        {
            Database?.Dispose();
        }
    }
}

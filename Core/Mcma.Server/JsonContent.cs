using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Mcma.Server
{
    public class JsonContent : StringContent
    {
        public JsonContent(object obj, JsonSerializerSettings settings = null)
            : this(JsonConvert.SerializeObject(obj, settings ?? new JsonSerializerSettings()))
        {
        }

        public JsonContent(string json)
            : base(json, Encoding.UTF8, "application/json")
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using JsonLD.Core;
using Microsoft.Extensions.Options;

namespace Mcma.JsonLd
{
    public class JsonLdContextManager : IJsonLdContextManager
    {
        /// <summary>
        /// Instantiates a <see cref="JsonLdContextManager"/>
        /// </summary>
        /// <param name="cacheOptions"></param>
        public JsonLdContextManager(IOptions<JsonLdContextManagerOptions> cacheOptions)
        {
            Options = cacheOptions?.Value ?? new JsonLdContextManagerOptions();
        }

        /// <summary>
        /// Gets the cache options
        /// </summary>
        private JsonLdContextManagerOptions Options { get; }

        /// <summary>
        /// Gets the context mappings dictionary
        /// </summary>
        private Dictionary<string, (Context, DateTime?)> ContextMappings { get; } = new Dictionary<string, (Context, DateTime?)>
        {
            {Contexts.Minimal.Url, (Contexts.Minimal.Context, null)},
            {Contexts.Default.Url, (Contexts.Default.Context, null)}
        };

        /// <summary>
        /// Gets or sets the default context url
        /// </summary>
        public string DefaultUrl
        {
            get => Options.DefaultContextUrl;
            set => Options.DefaultContextUrl = value;
        }

        /// <summary>
        /// Gets a JSON-LD context for a given url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Context Get(string url)
        {
            return ContextMappings.ContainsKey(url) ? ContextMappings[url].Item1 : null;
        }

        /// <summary>
        /// Sets the JSON-LD context for a given url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="context"></param>
        public void Set(string url, Context context)
        {
            ContextMappings[url] = (context, DateTime.UtcNow + Options.ItemLifetime);
        }

        /// <summary>
        /// Removes the JSON-LD context for a given url
        /// </summary>
        /// <param name="url"></param>
        public void Remove(string url)
        {
            if (ContextMappings.ContainsKey(url))
                ContextMappings.Remove(url);
        }

        /// <summary>
        /// Gets a JSON-LD context for a given url
        /// </summary>
        /// <returns></returns>
        public Context GetDefault()
        {
            return Get(DefaultUrl);
        }

        /// <summary>
        /// Sets the JSON-LD context for a given url
        /// </summary>
        /// <param name="context"></param>
        public void SetDefault(Context context)
        {
            Set(DefaultUrl, context);
        }

        /// <summary>
        /// Cleans expired contexts from the cache
        /// </summary>
        public void Clean()
        {
            // find old contexts based on expiration
            var expiredItems = ContextMappings.Where(c => c.Value.Item2.HasValue && c.Value.Item2 <= DateTime.UtcNow).ToList();
            if (!expiredItems.Any())
                return;

            // remove any expired items from the cache
            foreach (var expiredItem in expiredItems)
                ContextMappings.Remove(expiredItem.Key);
        }
    }
}
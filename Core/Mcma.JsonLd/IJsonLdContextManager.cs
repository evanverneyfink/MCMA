using JsonLD.Core;

namespace Mcma.JsonLd
{
    public interface IJsonLdContextManager
    {
        /// <summary>
        /// Gets or sets the default context url
        /// </summary>
        string DefaultUrl { get; set; }

        /// <summary>
        /// Gets a JSON-LD context for a given url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Context Get(string url);

        /// <summary>
        /// Sets the JSON-LD context for a given url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="context"></param>
        void Set(string url, Context context);

        /// <summary>
        /// Removes the JSON-LD context for a given url
        /// </summary>
        /// <param name="url"></param>
        void Remove(string url);

        /// <summary>
        /// Gets a JSON-LD context for a given url
        /// </summary>
        /// <returns></returns>
        Context GetDefault();

        /// <summary>
        /// Sets the JSON-LD context for a given url
        /// </summary>
        /// <param name="context"></param>
        void SetDefault(Context context);

        /// <summary>
        /// Cleans expired contexts from the cache
        /// </summary>
        void Clean();
    }
}

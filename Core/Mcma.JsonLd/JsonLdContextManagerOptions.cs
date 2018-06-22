using System;

namespace Mcma.JsonLd
{
    public class JsonLdContextManagerOptions
    {
        /// <summary>
        /// Gets or sets the default context url
        /// </summary>
        public string DefaultContextUrl { get; set; }

        /// <summary>
        /// Gets or sets the lifetime for a cache item
        /// </summary>
        public TimeSpan ItemLifetime { get; set; } = TimeSpan.FromMinutes(5);
    }
}
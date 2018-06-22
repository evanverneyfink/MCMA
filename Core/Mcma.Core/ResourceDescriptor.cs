using System;
using System.Collections.Generic;
using System.Linq;

namespace Mcma.Core
{
    public class ResourceDescriptor
    {
        /// <summary>
        /// Instantiates a <see cref="ResourceDescriptor"/>
        /// </summary>
        /// <param name="type"></param>
        public ResourceDescriptor(Type type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Gets or sets the type of the resource
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets or sets the ID of the resource, if any
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the resource descriptor of the parent, if any
        /// </summary>
        public ResourceDescriptor Parent { get; set; }

        /// <summary>
        /// Gets or sets the full url of the resource
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the parameters
        /// </summary>
        public string ParameterString { get; set; }

        /// <summary>
        /// Gets the parameters as a dictionary of key/value pairs
        /// </summary>
        public IDictionary<string, string> Parameters =>
            ParameterString?.Split('&')
                      .Select(x => x.Split('='))
                      .ToDictionary(x => x[0], x => x[1]);

        /// <summary>
        /// Gets the root type of the resource
        /// </summary>
        public Type RootType
        {
            get
            {
                var cur = this;

                while (cur.Parent != null)
                    cur = cur.Parent;

                return cur.Type;
            }
        }

        /// <summary>
        /// Creates a <see cref="ResourceDescriptor"/> for a given url and type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ResourceDescriptor FromUrl<T>(string url)
        {
            var uri = new Uri(url, UriKind.Absolute);

            return new ResourceDescriptor(typeof(T)) {Url = url, ParameterString = uri.Query};
        }
    }
}
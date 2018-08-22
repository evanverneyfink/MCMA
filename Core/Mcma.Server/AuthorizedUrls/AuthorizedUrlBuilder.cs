using System;
using System.Collections.Generic;
using System.Linq;
using Mcma.Core.Model;

namespace Mcma.Server.AuthorizedUrls
{
    internal class AuthorizedUrlBuilder : IAuthorizedUrlBuilder
    {
        /// <summary>
        /// Instantiates an <see cref="AuthorizedUrlBuilder"/>
        /// </summary>
        /// <param name="providerSpecificBuilders"></param>
        public AuthorizedUrlBuilder(IEnumerable<IProviderSpecificAuthorizedUrlBuilder> providerSpecificBuilders)
        {
            ProviderSpecificBuilders = providerSpecificBuilders?.ToList() ?? new List<IProviderSpecificAuthorizedUrlBuilder>();
        }

        /// <summary>
        /// Gets the provider-specific url builders
        /// </summary>
        private List<IProviderSpecificAuthorizedUrlBuilder> ProviderSpecificBuilders { get; }

        /// <summary>
        /// Gets the authorized url for accessing a service resource
        /// </summary>
        /// <param name="serviceResource"></param>
        /// <returns></returns>
        public string GetAuthorizedUrl(ServiceResource serviceResource)
        {
            if (string.IsNullOrWhiteSpace(serviceResource.AuthType))
                return serviceResource.HttpEndpoint;

            var builder = ProviderSpecificBuilders.FirstOrDefault(b => b.AuthType.Equals(serviceResource.AuthType));

            if (builder == null)
                throw new Exception($"Unknown auth type specified for service resource endpoint {serviceResource.HttpEndpoint}");

            return builder.GetAuthorizedUrl(serviceResource);
        }
    }
}
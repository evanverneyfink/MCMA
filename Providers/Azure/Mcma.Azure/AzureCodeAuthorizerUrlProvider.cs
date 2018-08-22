using Mcma.Core.Model;
using Mcma.Server.AuthorizedUrls;

namespace Mcma.Azure
{
    internal class AzureCodeAuthorizerUrlProvider : IProviderSpecificAuthorizedUrlBuilder
    {
        /// <summary>
        /// Gets the auth type as "AzureCode"
        /// </summary>
        public string AuthType { get; } = "AzureCode";

        /// <summary>
        /// Gets an authorized url for accessing an Azure resource
        /// </summary>
        /// <param name="serviceResource"></param>
        /// <returns></returns>
        public string GetAuthorizedUrl(ServiceResource serviceResource) => $"{serviceResource.HttpEndpoint}?code={serviceResource.AuthData}";
    }
}

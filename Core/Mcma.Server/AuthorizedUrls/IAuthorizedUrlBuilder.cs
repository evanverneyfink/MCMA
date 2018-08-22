using Mcma.Core.Model;

namespace Mcma.Server.AuthorizedUrls
{
    public interface IAuthorizedUrlBuilder
    {
        /// <summary>
        /// Gets the authorized url for accessing a service resource
        /// </summary>
        /// <param name="serviceResource"></param>
        /// <returns></returns>
        string GetAuthorizedUrl(ServiceResource serviceResource);
    }
}
namespace Mcma.Server.AuthorizedUrls
{
    public interface IProviderSpecificAuthorizedUrlBuilder : IAuthorizedUrlBuilder
    {
        /// <summary>
        /// Gets the type of auth this builder supports
        /// </summary>
        string AuthType { get; }
    }
}
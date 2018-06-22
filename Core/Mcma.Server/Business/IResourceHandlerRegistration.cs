namespace Mcma.Server.Business
{
    public interface IResourceHandlerRegistration
    {
        /// <summary>
        /// Registers
        /// </summary>
        /// <param name="options"></param>
        void Register(ResourceHandlerRegistryOptions options);
    }
}
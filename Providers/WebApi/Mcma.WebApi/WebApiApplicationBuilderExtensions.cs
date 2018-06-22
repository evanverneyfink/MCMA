using Mcma.Server.Environment;

namespace Mcma.WebApi
{
    public static class WebApiApplicationBuilderExtensions
    {
        /// <summary>
        /// The key for IIS express url setting
        /// </summary>
        private const string IisExpressUrlSettingKey = "iisSettings:iisExpress:applicationUrl";

        /// <summary>
        /// Adds the IIS express local url setting as an alternate key for the PublicUrl environment setting
        /// </summary>
        /// <param name="environmentOptions"></param>
        /// <returns></returns>
        public static EnvironmentOptions AddIisExpressUrl(this EnvironmentOptions environmentOptions)
        {
            environmentOptions.AddAlternateKey(nameof(EnvironmentExtensions.PublicUrl), IisExpressUrlSettingKey);
            return environmentOptions;
        }
    }
}
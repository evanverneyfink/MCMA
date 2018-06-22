using Microsoft.AspNetCore.Builder;

namespace Mcma.WebApi
{
    public static class McmaResourceApiMiddlewareExtensions
    {
        /// <summary>
        /// Uses FIMS API middleware
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMcmaWebApi(this IApplicationBuilder app)
        {
            return app.UseMiddleware<McmaResourceApiMiddleware>();
        }
    }
}
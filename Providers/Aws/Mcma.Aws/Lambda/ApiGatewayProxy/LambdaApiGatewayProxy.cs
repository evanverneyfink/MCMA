using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mcma.Aws.ServiceBuilding;
using Mcma.Server.Business;

namespace Mcma.Aws.Lambda.ApiGatewayProxy
{
    public static class LambdaApiGatewayProxy
    {
        /// <summary>
        /// Handles an API Gateway proxy request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaContext"></param>
        /// <param name="request"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static async Task<APIGatewayProxyResponse> Handle<T>(APIGatewayProxyRequest request,
                                                                    ILambdaContext lambdaContext,
                                                                    Action<McmaAwsServiceBuilder> configure = null)
            where T : IResourceHandlerRegistration, new()
        {
            IMcmaAwsResourceApi service = null;
            try
            {
                Console.WriteLine("Building FIMS service...");

                // build service
                var serviceBuilder = McmaAwsServiceBuilder.Create(opts => opts.AddProvider(new StageVariableProvider(request)))
                                               .With(lambdaContext);

                configure?.Invoke(serviceBuilder);
                    
                service = serviceBuilder.BuildResourceApi<ApiGatewayProxyLambdaRequest, T>();
            }
            catch (Exception exception)
            {
                // log error
                var message = $"An error occurred running API Gateway proxy lambda. Error: {exception}";
                if (service?.Logger != null)
                    service.Logger.Error(message);
                else
                    Console.WriteLine(message);

                // return unexpected 500
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = "An unexpected error occurred building the service."
                };
            }

            try
            {
                service.Logger.Info("Service built successfully. Starting request handling...");

                // handle request
                return await service.RequestHandler.HandleRequest(new ApiGatewayProxyLambdaRequest(request)) is IApiGatewayProxyLambdaResponse response
                           ? response.AsAwsApiGatewayProxyResponse()
                           : new APIGatewayProxyResponse
                           {
                               StatusCode = 500,
                               Body = "An unexpected error occurred. Internal configuration for API Gateway Proxy requests invalid."
                           };
            }
            catch (Exception exception)
            {
                // log error
                service.Logger.Error($"An error occurred running API Gateway proxy lambda. Error: {exception}");

                // return unexpected 500
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = "An unexpected error occurred processing the request."
                };
            }
            finally
            {
                service.Dispose();
            }
        }
    }
}

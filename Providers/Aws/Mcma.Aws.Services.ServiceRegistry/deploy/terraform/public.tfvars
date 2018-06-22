runtime = "dotnetcore2.0"

region = "us-east-1"

serviceName = "service-registry"
environmentName = "mcma-cloud-test"
environmentType = "dev"

restApiHandler = "Mcma.Aws.Services.ServiceRegistry::Mcma.Aws.Services.ServiceRegistry.ServiceRegistryFunctions::Api"
restApiZipFile = "../../bin/Release/netcoreapp2.0/publish/lambda/functions.zip"
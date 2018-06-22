runtime = "dotnetcore2.0"

region = "us-east-1"

serviceName = "job-repository"
environmentName = "mcma-cloud-test"
environmentType = "dev"

restApiHandler = "Mcma.Aws.Services.Jobs.JobRepository::Mcma.Aws.Services.Jobs.JobRepository.JobRepositoryFunctions::Api"
restApiZipFile = "../../bin/Release/netcoreapp2.0/publish/lambda/functions.zip"

serviceRegistryUrl = "https://dpzr6corrg.execute-api.us-east-1.amazonaws.com/dev"
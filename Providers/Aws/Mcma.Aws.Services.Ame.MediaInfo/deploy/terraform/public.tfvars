runtime = "dotnetcore2.0"

region = "us-east-1"

serviceName = "mediainfo-service"
environmentName = "mcma-cloud-test"
environmentType = "dev"

restApiHandler = "Mcma.Aws.Services.Ame.MediaInfo::Mcma.Aws.Services.Ame.MediaInfo.MediaInfoFunctions::JobApi"
restApiZipFile = "../../bin/Release/netcoreapp2.0/publish/lambda/functions.zip"

workerHandler = "Mcma.Aws.Services.Ame.MediaInfo::Mcma.Aws.Services.Ame.MediaInfo.MediaInfoFunctions::Worker"
workerZipFile = "../../bin/Release/netcoreapp2.0/publish/lambda/functions.zip"

serviceRegistryUrl = "https://dpzr6corrg.execute-api.us-east-1.amazonaws.com/dev"
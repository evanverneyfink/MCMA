# build code and package into zip for lambda
dotnet publish .. -c=Release
New-Item -ItemType Directory -Force -Path ..\bin\Release\netcoreapp2.0\publish\lambda
Compress-Archive -Path ..\bin\Release\netcoreapp2.0\publish\* -DestinationPath ..\bin\Release\netcoreapp2.0\publish\lambda\functions.zip -Force

# run terraform to deploy to AWS
cd terraform
terraform init -input=false
terraform apply -auto-approve -var-file="private.tfvars" -var-file="public.tfvars"
cd ..

# register the service and its profiles
Invoke-Expression -Command "./register.ps1"
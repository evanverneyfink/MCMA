# get terraform outputs
cd terraform
$apiUrl = terraform output restServiceUrl | Out-String
$serviceRegistryUrl = terraform output serviceRegistryUrl | Out-String
$apiUrl = $apiUrl -replace "`n","" -replace "`r",""
$serviceRegistryUrl = $serviceRegistryUrl -replace "`n","" -replace "`r",""
cd ..

# read terraform output and store it into the service.json
$serviceJson = (Get-Content ..\service.json | Out-String)
$serviceJson = $serviceJson.Replace("{{{serviceUrl}}}", $apiUrl)
$service = ConvertFrom-Json -InputObject $serviceJson

# get all the available job profiles from the service registry
$jobProfilesJson = Invoke-WebRequest -Method GET -Uri $serviceRegistryUrl"/JobProfiles"
$jobProfiles = ConvertFrom-Json $jobProfilesJson

# keep a collection of available profiles
$jobProfileUrls = New-Object System.Collections.ArrayList

# create or update each job profile
foreach ($jobProfile in $service.acceptsJobProfile)
{
	# check for existing profile with the same label
	$existingJobProfile = $null
	foreach ($tmpProfile in $jobProfiles)
	{
		if ($tmpProfile.label -eq $jobProfile.label)
		{
			$existingJobProfile = $tmpProfile
			break
		}
	}
	
	# convert the job profile back to JSON in order to send it as the body of a request
	$jobProfileJson = ConvertTo-Json $jobProfile

	# if there's no existing profile with the given label, we need to create it
	# otherwise we need to update it
	if ($existingJobProfile -eq $null)
	{
		# do a POST to create the new job profile and read the JSON response as an object
		$jobProfileResponse = Invoke-RestMethod -Method POST -Uri $serviceRegistryUrl"/JobProfiles" -Body $jobProfileJson

		# store the url of the new job profile
		$jobProfileUrls.Add($jobProfileResponse.id)
	}
	else
	{
		# do a PUT to update the existing job profile
		$jobProfileResponse = Invoke-RestMethod -Method PUT -Uri $existingJobProfile.id -Body $jobProfileJson

		# store the url of the existing job profile
		$jobProfileUrls.Add($existingJobProfile.id)
	}
}

# replace job profiles objects with links
$service.acceptsJobProfile = $jobProfileUrls.ToArray()

# convert the service back to JSON now that it has been updated
$serviceJson = ConvertTo-Json $service

# get existing services
$servicesJson = Invoke-WebRequest -Method GET -Uri $serviceRegistryUrl"/Services"
$services = ConvertFrom-Json $servicesJson

# check if the service already exists
$existingService = $null
foreach ($tmpService in $services)
{
	if ($tmpService.label -eq $service.label)
	{
		$existingService = $tmpService
		break
	}
}

# register the service with the service registry
if ($existingService -eq $null)
{
	Invoke-RestMethod -Method POST -Uri $serviceRegistryUrl"/Services" -Body $serviceJson
}
else
{
	Invoke-RestMethod -Method PUT -Uri $existingService.id -Body $serviceJson
}
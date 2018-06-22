
variable accessKey {}
variable secretKey {}
variable accountId {}
variable region {}

variable runtime {}

variable serviceName {}
variable environmentName {}
variable environmentType {}

variable restApiHandler {}
variable restApiZipFile {}

variable serviceRegistryUrl {}


module service {
  source = "./module"

  accessKey = "${var.accessKey}"
  secretKey = "${var.secretKey}"
  accountId = "${var.accountId}"
  region     = "${var.region}"

  runtime = "${var.runtime}"

  serviceName = "${var.serviceName}"
  environmentName = "${var.environmentName}"
  environmentType = "${var.environmentType}"
  serviceRegistryUrl = "${var.serviceRegistryUrl}"

  restApiHandler = "${var.restApiHandler}"
  restApiZipFile = "${var.restApiZipFile}"
}

output restServiceUrl {
	value = "${module.service.restServiceUrl}"
}

output serviceRegistryUrl {
	value = "${var.serviceRegistryUrl}"
}
variable accessKey {}
variable secretKey {}
variable accountId {}
variable subscriptionId {}
variable region {}

variable runtime {}

variable serviceName {}
variable environmentName {}
variable environmentType {}

variable restApiZipFile {}

terraform {
	backend "s3" {}
}

module "service" {
  source = "./module"

  accessKey      = "${var.accessKey}"
  secretKey      = "${var.secretKey}"
  accountId      = "${var.accountId}"
  subscriptionId = "${var.subscriptionId}"
  region         = "${var.region}"

  runtime = "${var.runtime}"

  serviceName     = "${var.serviceName}"
  environmentName = "${var.environmentName}"
  environmentType = "${var.environmentType}"

  restApiZipFile = "${var.restApiZipFile}"
}

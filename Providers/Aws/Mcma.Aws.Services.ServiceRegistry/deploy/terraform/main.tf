
variable access_key {}
variable secret_key {}
variable account_id {}
variable region {}

variable runtime {}

variable serviceName {}
variable environmentName {}
variable environmentType {}

variable restApiHandler {}
variable restApiZipFile {}

terraform {
	backend "s3" {}
}

module "service" {
  source = "./module"

  access_key = "${var.access_key}"
  secret_key = "${var.secret_key}"
  account_id = "${var.account_id}"
  region     = "${var.region}"

  runtime = "${var.runtime}"

  serviceName = "${var.serviceName}"
  environmentName = "${var.environmentName}"
  environmentType = "${var.environmentType}"

  restApiHandler = "${var.restApiHandler}"
  restApiZipFile = "${var.restApiZipFile}"
}
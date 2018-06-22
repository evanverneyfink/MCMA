variable "accessKey" {}
variable "secretKey" {}
variable "accountId" {}
variable "subscriptionId" {}
variable "region" {}

variable "runtime" {}

variable "serviceName" {}
variable "environmentName" {}
variable "environmentType" {}

variable "restApiZipFile" {}

locals {
  env_composite_name            = "${var.serviceName}-${var.environmentName}-${var.environmentType}"
  env_composite_name_lower_only = "${lower(var.serviceName)}${lower(var.environmentName)}${lower(var.environmentType)}"
}

provider "azurerm" {
  client_id       = "${var.accessKey}"
  client_secret   = "${var.secretKey}"
  tenant_id       = "${var.accountId}"
  subscription_id = "${var.subscriptionId}"
}

resource "azurerm_resource_group" "resource_group" {
  name     = "${local.env_composite_name}"
  location = "${var.region}"
}

resource "azurerm_storage_account" "storage_account" {
  name                = "${local.env_composite_name_lower_only}"
  resource_group_name = "${azurerm_resource_group.resource_group.name}"
  location            = "${azurerm_resource_group.resource_group.location}"

  account_tier             = "Standard"
  account_replication_type = "LRS"
}

data "azurerm_storage_account_sas" "storage_acct_sas" {
  connection_string = "${azurerm_storage_account.storage_account.primary_connection_string}"
  https_only        = true

  resource_types {
    service   = true
    container = false
    object    = true
  }

  services {
    blob  = true
    queue = false
    table = false
    file  = false
  }

  start  = "2016-01-01"
  expiry = "2050-12-31"

  permissions {
    read    = true
    write   = false
    delete  = false
    list    = false
    add     = false
    create  = false
    update  = false
    process = false
  }
}

# locals {
#   sas_split            = ["${split("&", data.azurerm_storage_account_sas.storage_acct_sas.sas)}"]
#   sas_sig_part         = "${local.sas_split[length(local.sas_split) - 1]}"
#   sas_sig_part_encoded = "sig=${urlencode(substr(local.sas_sig_part, 4, -1))}"
#   sas_encoded          = "${join("&", concat(slice(local.sas_split, 0, length(local.sas_split) - 1), list(local.sas_sig_part_encoded)))}"
# }

resource "azurerm_storage_container" "upload_container" {
  name                  = "src-upload"
  resource_group_name   = "${azurerm_resource_group.resource_group.name}"
  storage_account_name  = "${azurerm_storage_account.storage_account.name}"
  container_access_type = "private"
}

resource "azurerm_storage_blob" "uploaded_zip" {
  name = "${basename(var.restApiZipFile)}"

  resource_group_name    = "${azurerm_resource_group.resource_group.name}"
  storage_account_name   = "${azurerm_storage_account.storage_account.name}"
  storage_container_name = "${azurerm_storage_container.upload_container.name}"

  type   = "block"
  source = "${path.cwd}/${var.restApiZipFile}"
}

resource "azurerm_app_service_plan" "service_plan" {
  name                = "${local.env_composite_name}-svcplan"
  resource_group_name = "${azurerm_resource_group.resource_group.name}"
  location            = "${azurerm_resource_group.resource_group.location}"

  kind = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_application_insights" "app_insights" {
  name                = "${local.env_composite_name_lower_only}function"
  resource_group_name = "${azurerm_resource_group.resource_group.name}"
  location            = "${azurerm_resource_group.resource_group.location}"
  application_type    = "Web"
}

resource "azurerm_function_app" "api_function" {
  name                = "${local.env_composite_name_lower_only}function"
  resource_group_name = "${azurerm_resource_group.resource_group.name}"
  location            = "${azurerm_resource_group.resource_group.location}"

  app_service_plan_id       = "${azurerm_app_service_plan.service_plan.id}"
  storage_connection_string = "${azurerm_storage_account.storage_account.primary_connection_string}"
  version                   = "beta"

  app_settings {
    #WEBSITE_RUN_FROM_ZIP  = "${azurerm_storage_blob.uploaded_zip.url}${local.sas_encoded}"
    WEBSITE_RUN_FROM_ZIP           = "${azurerm_storage_blob.uploaded_zip.url}${data.azurerm_storage_account_sas.storage_acct_sas.sas}"
    APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.app_insights.instrumentation_key}"
    RootPath                       = "api/HandleRequest"
  }
}

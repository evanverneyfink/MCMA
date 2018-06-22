variable "accessKey" {}
variable "secretKey" {}
variable "accountId" {}
variable "region" {}

variable "runtime" {}

variable "serviceName" {}
variable "environmentName" {}
variable "environmentType" {}
variable "serviceRegistryUrl" {}

variable "restApiHandler" {}
variable "restApiZipFile" {}

variable "workerHandler" {}
variable "workerZipFile" {}

provider "aws" {
  access_key = "${var.accessKey}"
  secret_key = "${var.secretKey}"
  region     = "${var.region}"
}

locals {
  env_composite_name = "${var.serviceName}-${var.environmentName}-${var.environmentType}"
}

#################################
#  aws_iam_role : iam_for_exec_lambda
#################################

resource "aws_iam_role" "iam_for_exec_lambda" {
  name = "role_exec_lambda_${local.env_composite_name}"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "lambda.amazonaws.com"

      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF
}

resource "aws_iam_policy" "log_policy" {
  name        = "log_policy_${local.env_composite_name}"
  description = "Policy to write to log"

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "logs:*"
      ],
      "Effect": "Allow",
      "Resource": "*"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "role-policy-log" {
  role       = "${aws_iam_role.iam_for_exec_lambda.name}"
  policy_arn = "${aws_iam_policy.log_policy.arn}"
}

resource "aws_iam_policy" "DynamoDB_policy" {
  name        = "dynamodb_policy_${local.env_composite_name}"
  description = "Policy to Access DynamoDB"

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": "dynamodb:*",
      "Resource": "*"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "role-policy-DynamoDB" {
  role       = "${aws_iam_role.iam_for_exec_lambda.name}"
  policy_arn = "${aws_iam_policy.DynamoDB_policy.arn}"
}

resource "aws_iam_role_policy_attachment" "role-policy-lambda-full-access" {
  role       = "${aws_iam_role.iam_for_exec_lambda.name}"
  policy_arn = "arn:aws:iam::aws:policy/AWSLambdaFullAccess"
}

#################################
#  Lambda : rest-api-mcma_service_lambda
#################################

resource "aws_lambda_function" "api_mcma_service_lambda" {
  filename         = "${var.restApiZipFile}"
  function_name    = "${local.env_composite_name}"
  role             = "${aws_iam_role.iam_for_exec_lambda.arn}"
  handler          = "${var.restApiHandler}"
  source_code_hash = "${base64sha256(file("${var.restApiZipFile}"))}"
  runtime          = "${var.runtime}"
  timeout          = "60"
  memory_size      = "1024"

  environment {
    variables = {
      WorkerFunctionName = "${local.env_composite_name}_worker"
    }
  }
}

#################################
#  Lambda : worker-mcma_service_lambda
#################################

resource "aws_lambda_function" "worker_lambda" {
  filename         = "${var.workerZipFile}"
  function_name    = "${local.env_composite_name}_worker"
  role             = "${aws_iam_role.iam_for_exec_lambda.arn}"
  handler          = "${var.workerHandler}"
  source_code_hash = "${base64sha256(file("${var.workerZipFile}"))}"
  runtime          = "${var.runtime}"
  timeout          = "60"
  memory_size      = "1024"
}

##################################
# aws_dynamodb_table : repo_service_table
##################################

resource "aws_dynamodb_table" "repo_service_table" {
  name           = "${local.env_composite_name}"
  read_capacity  = 1
  write_capacity = 1
  hash_key       = "resource_type"
  range_key      = "resource_id"

  attribute {
    name = "resource_type"
    type = "S"
  }

  attribute {
    name = "resource_id"
    type = "S"
  }

  tags {
    framework = "FIMSCLOUD"
    version   = "V1.0"
    author    = "Loic Barbou"
  }
  
  stream_enabled = true
  stream_view_type = "NEW_IMAGE"


}

##############################
#  API Gateway
##############################
resource "aws_api_gateway_rest_api" "mcma_service_api" {
  name        = "${local.env_composite_name}"
}

resource "aws_api_gateway_resource" "mcma_service_api_resource" {
  rest_api_id = "${aws_api_gateway_rest_api.mcma_service_api.id}"
  parent_id   = "${aws_api_gateway_rest_api.mcma_service_api.root_resource_id}"
  path_part   = "{proxy+}"
}

resource "aws_api_gateway_method" "mcma_service_api_method" {
  rest_api_id   = "${aws_api_gateway_rest_api.mcma_service_api.id}"
  resource_id   = "${aws_api_gateway_resource.mcma_service_api_resource.id}"
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "mcma_service_api_method-integration" {
  rest_api_id             = "${aws_api_gateway_rest_api.mcma_service_api.id}"
  resource_id             = "${aws_api_gateway_resource.mcma_service_api_resource.id}"
  http_method             = "${aws_api_gateway_method.mcma_service_api_method.http_method}"
  type                    = "AWS_PROXY"
  uri                     = "arn:aws:apigateway:${var.region}:lambda:path/2015-03-31/functions/arn:aws:lambda:${var.region}:${var.accountId}:function:${aws_lambda_function.api_mcma_service_lambda.function_name}/invocations"
  integration_http_method = "POST"
}

resource "aws_lambda_permission" "apigw_lambda" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = "${aws_lambda_function.api_mcma_service_lambda.arn}"
  principal     = "apigateway.amazonaws.com"

  # More: http://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-control-access-using-iam-policies-to-invoke-api.html
  source_arn = "arn:aws:execute-api:${var.region}:${var.accountId}:${aws_api_gateway_rest_api.mcma_service_api.id}/*/${aws_api_gateway_method.mcma_service_api_method.http_method}/*"
}

resource "aws_api_gateway_deployment" "mcma_service_deployment" {
  depends_on = [
    "aws_api_gateway_method.mcma_service_api_method",
    "aws_api_gateway_integration.mcma_service_api_method-integration",
  ]

  rest_api_id = "${aws_api_gateway_rest_api.mcma_service_api.id}"
  stage_name  = "${var.environmentType}"

  variables = {
    "TableName"                = "${local.env_composite_name}"
    "PublicUrl"                = "https://${aws_api_gateway_rest_api.mcma_service_api.id}.execute-api.${var.region}.amazonaws.com/${var.environmentType}"
    "ServiceRegistryUrl"       = "${var.serviceRegistryUrl}"
    "WorkerFunctionName"       = "${aws_lambda_function.worker_lambda.function_name}"
  }
}

##################################
# Output 
##################################

output restServiceUrl {
  value = "https://${aws_api_gateway_deployment.mcma_service_deployment.rest_api_id}.execute-api.${var.region}.amazonaws.com/${aws_api_gateway_deployment.mcma_service_deployment.stage_name}"
}
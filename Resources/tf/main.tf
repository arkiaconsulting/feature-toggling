provider "azurerm" {}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "rg" {
  name     = "${var.prefix}"
  location = "${var.location}"
}

data "azuread_user" "current_user" {
  user_principal_name = "${var.user_upn}"
}

resource "azurerm_key_vault" "safe" {
  name                = "${var.prefix}"
  location            = "${azurerm_resource_group.rg.location}"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  tenant_id           = "${data.azurerm_client_config.current.tenant_id}"

  sku {
    name = "standard"
  }
}

resource "azurerm_key_vault_access_policy" "policy_user" {
  key_vault_id = "${azurerm_key_vault.safe.id}"

  tenant_id = "${data.azurerm_client_config.current.tenant_id}"
  object_id = "${data.azuread_user.current_user.id}"

  key_permissions = []

  secret_permissions = [
    "get",
    "list",
    "set",
    "delete",
  ]
}

resource "azurerm_key_vault_access_policy" "app_policy" {
  key_vault_id = "${azurerm_key_vault.safe.id}"

  tenant_id = "${data.azurerm_client_config.current.tenant_id}"
  object_id = "${azurerm_function_app.function.identity.0.principal_id}"

  key_permissions = []

  secret_permissions = [
    "get",
    "list",
  ]
}

resource "azurerm_storage_account" "storage" {
  name                     = "${replace(var.prefix, "-", "")}"
  resource_group_name      = "${azurerm_resource_group.rg.name}"
  location                 = "${azurerm_resource_group.rg.location}"
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "web" {
  name                  = "web"
  resource_group_name   = "${azurerm_resource_group.rg.name}"
  storage_account_name  = "${azurerm_storage_account.storage.name}"
  container_access_type = "container"
}

resource "azurerm_app_service_plan" "asp" {
  name                = "${var.prefix}"
  location            = "${azurerm_resource_group.rg.location}"
  resource_group_name = "${azurerm_resource_group.rg.name}"

  sku {
    tier = "Shared"
    size = "D1"
  }
}

resource "azurerm_function_app" "function" {
  name                      = "${var.prefix}"
  location                  = "${azurerm_resource_group.rg.location}"
  resource_group_name       = "${azurerm_resource_group.rg.name}"
  app_service_plan_id       = "${azurerm_app_service_plan.asp.id}"
  storage_connection_string = "${azurerm_storage_account.storage.primary_connection_string}"
  version                   = "~2"

  identity {
    type = "SystemAssigned"
  }

  app_settings {
    "AppConfigEndpoint"                     = "${lookup(azurerm_template_deployment.app_config.outputs, "endpoint")}"
    "FeatureToggling:OpenWeatherMap:ApiKey" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.open_weather_map_api_key.id})"
    "FeatureToggling:AccuWeather:ApiKey"    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.accu_weather_api_key.id})"
  }
}

resource "azurerm_template_deployment" "app_config" {
  name                = "appconfigdeploy"
  resource_group_name = "${azurerm_resource_group.rg.name}"

  template_body = "${file("arm/app-config.json")}"

  parameters = {
    "name" = "${var.prefix}"
  }

  deployment_mode = "Incremental"
}

resource "azurerm_key_vault_secret" "open_weather_map_api_key" {
  name         = "FeatureToggling--OpenWeatherMap--ApiKey"
  value        = "${var.open_weather_map_api_key}"
  key_vault_id = "${azurerm_key_vault.safe.id}"
}

resource "azurerm_key_vault_secret" "accu_weather_api_key" {
  name         = "FeatureToggling--AccuWeather--ApiKey"
  value        = "${var.accu_weather_api_key}"
  key_vault_id = "${azurerm_key_vault.safe.id}"
}

resource "azurerm_template_deployment" "cors" {
  name                = "arm_cors"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  deployment_mode     = "Incremental"

  template_body = "${file("arm/cors.json")}"

  parameters {
    "name"          = "${azurerm_function_app.function.name}"
    "allowedOrigin" = "${substr(azurerm_storage_account.storage.primary_blob_endpoint, 0, length(azurerm_storage_account.storage.primary_blob_endpoint) - 1)}"
  }
}

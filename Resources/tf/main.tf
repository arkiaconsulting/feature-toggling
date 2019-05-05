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

  identity {
    type = "SystemAssigned"
  }

  app_settings {
    KeyVaultUri = "${azurerm_key_vault.safe.vault_uri}"
  }
}

resource "azurerm_key_vault_secret" "app_configuration_connection_string" {
  name         = "AppConfigConnectionString"
  value        = "${lookup(azurerm_template_deployment.app_config.outputs, "connectionString")}"
  key_vault_id = "${azurerm_key_vault.safe.id}"
}

resource "azurerm_template_deployment" "app_config" {
  name                = "appconfigdeploy"
  resource_group_name = "${azurerm_resource_group.rg.name}"

  template_body = <<DEPLOY
  {
      "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
      "contentVersion": "1.0.0.0",
      "parameters": {
          "name": {
              "type": "string"
          }
      },
      "resources": [          
            {
                "type": "Microsoft.AppConfiguration/configurationStores",
                "name": "[parameters('name')]",
                "apiVersion": "2019-02-01-preview",
                "location": "westeurope"
            }
      ],
      "outputs":{
          "connectionString": {
              "type": "string",
              "value":"[listKeys(resourceId('Microsoft.AppConfiguration/configurationStores', parameters('name')), '2019-02-01-preview').value[0].connectionString]"
          }
      }
  }
DEPLOY

  parameters = {
    "name" = "${var.prefix}"
  }

  deployment_mode = "Incremental"
}

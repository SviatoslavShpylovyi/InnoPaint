resource "random_string" "suffix" {
  length  = 5
  special = false
  upper   = false
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${var.prefix}"
  location = var.location

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${var.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_container_registry" "main" {
  name                = "acr${replace(var.prefix, "-", "")}${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = false

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_user_assigned_identity" "pull" {
  name                = "id-${var.prefix}-pull"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.pull.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_container_app_environment" "main" {
  name                       = "cae-${var.prefix}"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_mysql_flexible_server" "main" {
  name                = "mysql-${var.prefix}-${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  administrator_login    = var.mysql_admin_login
  administrator_password = var.mysql_admin_password

  sku_name = "B_Standard_B1ms"
  version  = "8.4"

  storage {
    size_gb = 20
  }

  backup_retention_days = 7

  lifecycle {
    ignore_changes = [zone]
  }

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_mysql_flexible_database" "main" {
  name                = "InnoPaint"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.main.name

  charset   = "utf8mb4"
  collation = "utf8mb4_0900_ai_ci"
}

resource "azurerm_mysql_flexible_server_firewall_rule" "azure_services" {
  name                = "AllowAzureServices"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.main.name

  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

locals {
  mysql_connection_string = "Server=${azurerm_mysql_flexible_server.main.fqdn};Port=3306;Database=${azurerm_mysql_flexible_database.main.name};User=${var.mysql_admin_login};Password=${var.mysql_admin_password};SslMode=Required"
}

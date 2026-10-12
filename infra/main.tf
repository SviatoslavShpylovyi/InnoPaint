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
  daily_quota_gb      = 0.1

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

# Without an explicit mode Azure creates the environment as "Express", which
# has no jobs, no insecure ingress and no service discovery. azurerm 4.x has no
# argument for the mode, so it is set through the API right after creation.
resource "azapi_resource_action" "environment_mode" {
  type        = "Microsoft.App/managedEnvironments@2026-07-01"
  resource_id = azurerm_container_app_environment.main.id
  method      = "PATCH"

  body = {
    properties = {
      environmentMode = "WorkloadProfiles"
    }
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

resource "azurerm_container_app" "api" {
  name                         = "api"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.pull.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.pull.id
  }

  secret {
    name  = "connection-string"
    value = local.mysql_connection_string
  }

  secret {
    name  = "openai-api-key"
    value = var.openai_api_key
  }

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "api"
      image  = "${azurerm_container_registry.main.login_server}/innopaint/api:${var.image_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "ConnectionStrings__InnoPaint"
        secret_name = "connection-string"
      }

      env {
        name        = "OpenAI__ApiKey"
        secret_name = "openai-api-key"
      }

      env {
        name  = "OpenAI__Model"
        value = var.openai_model
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      readiness_probe {
        transport = "HTTP"
        path      = "/health"
        port      = 8080
        timeout   = 5
      }
    }
  }

  ingress {
    external_enabled           = false
    allow_insecure_connections = true
    target_port                = 8080

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }

  depends_on = [
    azurerm_role_assignment.acr_pull,
    azapi_resource_action.environment_mode
  ]

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_container_app" "web" {
  name                         = "web"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.pull.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.pull.id
  }

  secret {
    name  = "basic-auth-htpasswd"
    value = var.basic_auth_htpasswd
  }

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "web"
      image  = "${azurerm_container_registry.main.login_server}/innopaint/web:${var.image_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "API_UPSTREAM"
        value = "http://api"
      }

      env {
        name        = "BASIC_AUTH_HTPASSWD"
        secret_name = "basic-auth-htpasswd"
      }

      readiness_probe {
        transport = "HTTP"
        path      = "/healthz"
        port      = 80
        timeout   = 5
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 80

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }

    dynamic "ip_security_restriction" {
      for_each = var.allowed_ips

      content {
        name             = "allow-${ip_security_restriction.key}"
        action           = "Allow"
        ip_address_range = ip_security_restriction.value
      }
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }

  depends_on = [
    azurerm_role_assignment.acr_pull,
    azurerm_container_app.api
  ]

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_container_app_job" "migrate" {
  name                         = "migrate"
  location                     = azurerm_resource_group.main.location
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  workload_profile_name        = "Consumption"

  replica_timeout_in_seconds = 600
  replica_retry_limit        = 1

  manual_trigger_config {
    parallelism              = 1
    replica_completion_count = 1
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.pull.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.pull.id
  }

  secret {
    name  = "connection-string"
    value = local.mysql_connection_string
  }

  template {
    container {
      name   = "migrate"
      image  = "${azurerm_container_registry.main.login_server}/innopaint/migrate:${var.image_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "ConnectionStrings__InnoPaint"
        secret_name = "connection-string"
      }
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].image
    ]
  }

  depends_on = [
    azurerm_role_assignment.acr_pull,
    azapi_resource_action.environment_mode
  ]

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_user_assigned_identity" "github" {
  name                = "id-${var.prefix}-github"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  tags = {
    project = "innopaint"
    env     = "dev"
  }
}

resource "azurerm_federated_identity_credential" "github_main" {
  name                      = "github-main"
  user_assigned_identity_id = azurerm_user_assigned_identity.github.id

  issuer   = "https://token.actions.githubusercontent.com"
  audience = ["api://AzureADTokenExchange"]
  subject  = "repo:${var.github_repo}:ref:refs/heads/main"
}

resource "azurerm_role_assignment" "github_contributor" {
  scope                = azurerm_resource_group.main.id
  role_definition_name = "Contributor"
  principal_id         = azurerm_user_assigned_identity.github.principal_id
  principal_type       = "ServicePrincipal"
}

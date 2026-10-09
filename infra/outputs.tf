output "acr_name" {
  value = azurerm_container_registry.main.name
}

output "acr_login_server" {
  value = azurerm_container_registry.main.login_server
}

output "mysql_fqdn" {
  value = azurerm_mysql_flexible_server.main.fqdn
}
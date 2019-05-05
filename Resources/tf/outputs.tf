output "open_weather_map_endpoint" {
  value = "${var.open_weather_map_endpoint}"
}

output "accu_weather_endpoint" {
  value = "${var.accu_weather_endpoint}"
}

output "app_config_name" {
  value = "${lookup(azurerm_template_deployment.app_config.outputs, "name")}"
}

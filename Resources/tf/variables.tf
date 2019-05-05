variable "prefix" {}

variable "location" {}

variable "user_upn" {}
variable "open_weather_map_api_key" {}
variable "accu_weather_api_key" {}

variable "open_weather_map_endpoint" {
  default = "https://api.openweathermap.org/data/2.5/weather"
}

variable "accu_weather_endpoint" {
  default = "https://dataservice.accuweather.com"
}

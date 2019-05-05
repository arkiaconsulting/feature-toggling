using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureToggling.Services
{
    public class AccuWeatherService : IWeatherService
    {
        private readonly AccuWeatherOptions _options;

        public AccuWeatherService(IOptions<AccuWeatherOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<WeatherDto> GetWeather(string location)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = _options.Endpoint;
                var locationResponse = (await client.GetAs<AccuWeatherLocationResponse[]>($"/locations/v1/cities/search?apikey={_options.ApiKey}&q={location}&language=fr-fr")).First();

                var weatherResponse = (await client.GetAs<AccuWeatherWeatherResponse[]>($"/currentconditions/v1/{locationResponse.Key}?apikey={_options.ApiKey}&q={location}&language=fr-fr&details=true")).First();

                return new WeatherDto
                {
                    Source = "AccuWeather",
                    Temperature = weatherResponse.Enveloppe.Temperature.Value,
                    CloudCover = weatherResponse.CloudCover
                };
            }
        }
    }

    public class AccuWeatherOptions
    {
        public string ApiKey { get; set; }
        public Uri Endpoint { get; set; }
    }

    class AccuWeatherLocationResponse
    {
        [JsonProperty("Key")]
        public string Key { get; set; }
    }

    class AccuWeatherWeatherResponse
    {
        [JsonProperty("Temperature")]
        public AccuWeatherTemperatureEnveloppe Enveloppe { get; set; }

        [JsonProperty("CloudCover")]
        public int CloudCover { get; set; }
    }

    class AccuWeatherTemperatureEnveloppe
    {
        [JsonProperty("Metric")]
        public AccuWeatherTemperature Temperature { get; set; }
    }

    class AccuWeatherTemperature
    {
        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}

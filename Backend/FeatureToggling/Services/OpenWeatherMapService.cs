using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureToggling.Services
{
    public class OpenWeatherMapService : IWeatherService
    {
        private readonly OpenWeatherMapOptions _options;

        public OpenWeatherMapService(IOptions<OpenWeatherMapOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<WeatherDto> GetWeather(string location)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = _options.Endpoint;

                var dto = await client.GetAs<OpenWeatherMapResponse>($"?q={location}&units=metric&appid={_options.ApiKey}");

                return new WeatherDto
                {
                    Source = "OpenWeatherMap",
                    Temperature = dto.Main.Temperature.ToString(),
                    CloudCover = dto.Clouds.All
                };
            }
        }
    }

    public class OpenWeatherMapOptions
    {
        public string ApiKey { get; set; }
        public Uri Endpoint { get; set; }
    }

    class OpenWeatherMapResponse
    {
        [JsonProperty("main")]
        public OpenWeatherMapMain Main { get; set; }

        [JsonProperty("clouds")]
        public OpenWeatherCloud Clouds { get; set; }
    }

    class OpenWeatherCloud
    {
        [JsonProperty("all")]
        public int All { get; set; }
    }

    class OpenWeatherMapMain
    {
        [JsonProperty("temp")]
        public decimal Temperature { get; set; }
    }
}

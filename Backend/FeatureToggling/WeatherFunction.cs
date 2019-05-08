using FeatureToggling.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FeatureToggling
{
    public class WeatherFunction
    {
        private readonly IWeatherService _weatherService;

        public WeatherFunction(IWeatherService weatherService)
        {
            _weatherService = weatherService ?? throw new System.ArgumentNullException(nameof(weatherService));
        }

        [FunctionName("Weather")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather")] HttpRequest req,
            ILogger log)
        {
            string location = req.Query["location"];

            log.LogInformation($"Weather was asked for location {location}");

            var weather = await _weatherService.GetWeather(location);

            return location != null
                ? (ActionResult)new OkObjectResult(weather)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}

using System.Threading.Tasks;

namespace FeatureToggling.Services
{
    public interface IWeatherService
    {
        Task<WeatherDto> GetWeather(string location);
    }
}

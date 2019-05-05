using FeatureToggling;
using FeatureToggling.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace FeatureToggling
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) => builder.UseDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OpenWeatherMapOptions>(configuration.GetSection("FeatureToggling:OpenWeatherMap"));
            services.Configure<AccuWeatherOptions>(configuration.GetSection("FeatureToggling:AccuWeather"));
            //services.AddScoped<IWeatherService, OpenWeatherMapService>();
            services.AddScoped<IWeatherService, AccuWeatherService>();
        }
    }
}

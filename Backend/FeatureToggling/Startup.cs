using FeatureToggling;
using FeatureToggling.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace FeatureToggling
{
    public class Startup : FunctionsStartup
    {
        private IConfiguration Configuration { get; set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.UseDependencyInjection(
                ConfigureConfigurationBuilder,
                config => Configuration = config);

            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.Configure<OpenWeatherMapOptions>(Configuration.GetSection("FeatureToggling:OpenWeatherMap"));
            services.Configure<AccuWeatherOptions>(Configuration.GetSection("FeatureToggling:AccuWeather"));
            services.AddFeatureTogglingWeatherServices("FeatureToggling:WeatherSource");
        }

        private void ConfigureConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddFeatureTogglingAppConfiguration();
        }
    }
}

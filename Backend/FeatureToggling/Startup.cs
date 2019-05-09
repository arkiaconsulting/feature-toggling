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
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.UseDependencyInjection(ConfigureConfigurationBuilder);

            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<OpenWeatherMapOptions>()
                .Configure<IConfiguration>((opts, config) => config.Bind("FeatureToggling:OpenWeatherMap", opts));
            services.AddOptions<AccuWeatherOptions>()
                .Configure<IConfiguration>((opts, config) => config.Bind("FeatureToggling:AccuWeather", opts));
            services.AddFeatureTogglingWeatherServices("FeatureToggling:WeatherSource");
        }

        private void ConfigureConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddFeatureTogglingAppConfiguration();
        }
    }
}

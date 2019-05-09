using FeatureToggling.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace FeatureToggling
{
    public static class StartupExtensions
    {
        public static void UseDependencyInjection(
            this IFunctionsHostBuilder builder,
            Action<IConfigurationBuilder> configureConfigurationBuilder
            )
        {
            var configurationBuilder = new ConfigurationBuilder();
            var existingConfigurationDescriptor = builder.Services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IConfiguration));
            if (existingConfigurationDescriptor?.ImplementationInstance is IConfigurationRoot)
            {
                // Merge the current config into a new ConfigurationBuilder to keep the existing configuration
                configurationBuilder.AddConfiguration(existingConfigurationDescriptor.ImplementationInstance as IConfigurationRoot);
            }

            configureConfigurationBuilder(configurationBuilder);

            if (existingConfigurationDescriptor?.ImplementationInstance is IConfigurationRoot)
            {
                //replace the existing config with the new one
                builder.Services.Replace(new ServiceDescriptor(typeof(IConfiguration), _ => configurationBuilder.Build(), existingConfigurationDescriptor.Lifetime));
            }
            else
            {
                builder.Services.AddScoped<IConfiguration>(sp => configurationBuilder.Build());
            }
        }

        public static IConfigurationBuilder AddFeatureTogglingAppConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options
                    .ConnectWithManagedIdentity(Environment.GetEnvironmentVariable("AppConfigEndpoint"))
                    .Use(keyFilter: "FeatureToggling:*")
                    .Watch("FeatureToggling:WeatherSource", TimeSpan.FromSeconds(5))
                    .UseFeatureFlags();
            });

            return configurationBuilder;
        }

        public static IServiceCollection AddFeatureTogglingWeatherServices(this IServiceCollection services, string weatherSourceConfigurationName)
        {
            services.AddScoped(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var typeName = $"{configuration[weatherSourceConfigurationName]}Service";
                var type = Assembly.GetExecutingAssembly().ExportedTypes.Single(t => t.Name == typeName);
                return (IWeatherService)sp.GetRequiredService(type);
            });
            services.AddTransient<OpenWeatherMapService>();
            services.AddTransient<AccuWeatherService>();

            return services;
        }
    }
}

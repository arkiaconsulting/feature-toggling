using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace FeatureToggling
{
    public static class StartupExtensions
    {
        public static void UseDependencyInjection(this IWebJobsBuilder builder, Action<IServiceCollection,IConfiguration> configureServices)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var existingConfigurationDescriptor = builder.Services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IConfiguration));
            if (existingConfigurationDescriptor?.ImplementationInstance is IConfigurationRoot)
            {
                // Merge the current config into a new ConfigurationBuilder to keep the existing configuration
                configurationBuilder.AddConfiguration(existingConfigurationDescriptor.ImplementationInstance as IConfigurationRoot);
            }
            
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.ConnectWithManagedIdentity(Environment.GetEnvironmentVariable("AppConfigEndpoint"));
                options.Use(keyFilter: "FeatureToggling:*");
            });

            var config = configurationBuilder.Build();
            if (existingConfigurationDescriptor?.ImplementationInstance is IConfigurationRoot)
            {
                //replace the existing config with the new one
                builder.Services.Replace(new ServiceDescriptor(typeof(IConfiguration), _ => config, existingConfigurationDescriptor.Lifetime));
            }
            else
            {
                builder.Services.AddScoped<IConfiguration>(sp => config);
            }

            configureServices(builder.Services, config);
        }
    }
}

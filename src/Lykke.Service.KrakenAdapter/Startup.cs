using System;
using JetBrains.Annotations;
using Lykke.Logs;
using Lykke.Sdk;
using Lykke.Service.KrakenAdapter.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lykke.Service.KrakenAdapter
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "KrakenAdapterService"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.AzureTableName = "KrakenAdapterLog";
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.KrakenAdapterService.Db.LogsConnString;

                    // TODO: You could add extended logging configuration here:
                    /*
                    logs.Extended = extendedLogs =>
                    {
                        // For example, you could add additional slack channel like this:
                        extendedLogs.AddAdditionalSlackChannel("TestAdapter", channelOptions =>
                        {
                            channelOptions.MinLogLevel = LogLevel.Information;
                        });
                    };
                    */
                };

                options.SwaggerOptions = _swaggerOptions;
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });
        }
    }
}

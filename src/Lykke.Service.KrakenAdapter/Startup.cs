using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Sdk;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
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
                };

                options.Swagger = swagger => swagger.ConfigureSwagger();
                options.SwaggerOptions = _swaggerOptions;
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<TradingApiSettings>();
            var logFactory = app.ApplicationServices.GetService<ILogFactory>();

            XApiKeyAuthAttribute.Credentials =
                settings.Credentials.ToDictionary(x => x.InternalApiKey, x => (object) x);

            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
                
                options.WithMiddleware = x =>
                {
                    x.UseAuthenticationMiddleware(token => new RestClient(logFactory, GetCredentials(settings, token)));
                    x.UseHandleBusinessExceptionsMiddleware();
                    x.UseForwardBitstampExceptionsMiddleware();
                };
            });
        }

        private ApiCredentials GetCredentials(TradingApiSettings settings, string token)
        {
            var s = settings.Credentials.FirstOrDefault(x => x.InternalApiKey == token);

            if (s == null) return null;

            return new ApiCredentials(s.KrakenApiKey, s.KrakenApiSecret);
        }
    }
}

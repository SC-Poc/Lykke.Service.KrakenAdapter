using System;
using System.Linq;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Sdk;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.KrakenAdapter
{
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "KrakenAdapterService API",
            ApiVersion = "v1"
        };

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
                    x.UseForwardKrakenExceptionsMiddleware();
                };
            });
        }

        private static ApiCredentials GetCredentials(TradingApiSettings settings, string token)
        {
            Credentials credentials = settings.Credentials.FirstOrDefault(x => x.InternalApiKey == token);

            if (credentials == null)
                return null;

            return new ApiCredentials(credentials.KrakenApiKey, credentials.KrakenApiSecret);
        }
    }
}

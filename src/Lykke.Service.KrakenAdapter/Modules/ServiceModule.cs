using System.Collections.Generic;
using Autofac;
using Lykke.Common.ExchangeAdapter.Server.Settings;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.KrakenAdapter.Modules
{    
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrderBooksPublishingService>()
                .WithParameter(
                    new TypedParameter(
                        typeof(KrakenOrderBookProcessingSettings),
                        _appSettings.CurrentValue.KrakenAdapterService.OrderBooks))
                .AsSelf()
                .As<IHostedService>()
                .SingleInstance();

            builder.RegisterInstance(_appSettings.CurrentValue.KrakenAdapterService.TradingApi).AsSelf();

            builder.RegisterType<KrakenInformationService>()
                .AsSelf()
                .As<IHostedService>()
                .SingleInstance();

            // Do not register entire settings in container, pass necessary settings to services which requires them
        }
    }
}

using Autofac;
using JetBrains.Annotations;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.KrakenAdapter.Modules
{
    [UsedImplicitly]
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

            builder.RegisterInstance(_appSettings.CurrentValue.KrakenAdapterService.TradingApi)
                .AsSelf();

            builder.RegisterType<KrakenInformationService>()
                .AsSelf()
                .As<IHostedService>()
                .SingleInstance();
        }
    }
}

using JetBrains.Annotations;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Settings.ServiceSettings.Db;
using Lykke.Service.KrakenAdapter.Settings.ServiceSettings.TradingApi;

namespace Lykke.Service.KrakenAdapter.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class KrakenAdapterSettings
    {
        public DbSettings Db { get; set; }

        public KrakenOrderBookProcessingSettings OrderBooks { get; set; }

        public TradingApiSettings TradingApi { get; set; }
    }
}

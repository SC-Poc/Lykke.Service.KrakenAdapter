using JetBrains.Annotations;
using Lykke.Service.KrakenAdapter.Services;

namespace Lykke.Service.KrakenAdapter.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class KrakenAdapterSettings
    {
        public DbSettings Db { get; set; }
        public KrakenOrderBookProcessingSettings OrderBooks { get; set; }
        public TradingApiSettings TradingApi { get; set; }
    }
}

using JetBrains.Annotations;

namespace Lykke.Service.KrakenAdapter.Settings.ServiceSettings.TradingApi
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RetrySettings
    {
        public int Count { get; set; }

        public int Delay { get; set; }
    }
}

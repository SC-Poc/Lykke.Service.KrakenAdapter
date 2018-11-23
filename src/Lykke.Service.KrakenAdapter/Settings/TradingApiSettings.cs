using JetBrains.Annotations;

namespace Lykke.Service.KrakenAdapter.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TradingApiSettings
    {
        public Credentials[] Credentials { get; set; }
    }
}

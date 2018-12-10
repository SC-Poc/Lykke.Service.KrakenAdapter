using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KrakenAdapter.Settings.ServiceSettings.TradingApi
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TradingApiSettings
    {
        public TradingApiSettings()
        {
            Retry = new RetrySettings
            {
                Count = 3,
                Delay = 1000
            };
        }

        public Credentials[] Credentials { get; set; }

        [Optional]
        public RetrySettings Retry { get; set; }
    }
}

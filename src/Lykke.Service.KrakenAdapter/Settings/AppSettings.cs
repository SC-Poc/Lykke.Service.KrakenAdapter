using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.KrakenAdapter.Services;

namespace Lykke.Service.KrakenAdapter.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public KrakenAdapterSettings KrakenAdapterService { get; set; }        
    }
}

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KrakenAdapter.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}

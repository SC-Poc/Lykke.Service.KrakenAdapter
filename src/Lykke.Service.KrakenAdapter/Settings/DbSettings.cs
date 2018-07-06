using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KrakenAdapter.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}

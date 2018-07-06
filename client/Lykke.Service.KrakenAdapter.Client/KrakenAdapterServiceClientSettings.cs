using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KrakenAdapter.Client 
{
    /// <summary>
    /// KrakenAdapter client settings.
    /// </summary>
    public class KrakenAdapterServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}

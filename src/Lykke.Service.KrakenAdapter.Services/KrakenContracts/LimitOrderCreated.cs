using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class LimitOrderCreated
    {
        [JsonProperty("txid")]
        public string[] Txid { get; set; }
    }
}
using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public sealed class LimitOrderCreated
    {
        [JsonProperty("txid")]
        public string[] Txid { get; set; }
    }
}

using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public sealed class OrdersCanceled
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}

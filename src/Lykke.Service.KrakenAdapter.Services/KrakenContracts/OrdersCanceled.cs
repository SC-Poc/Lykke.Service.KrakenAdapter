using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class OrdersCanceled
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
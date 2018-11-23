using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public sealed class AsksAndBids
    {
        [JsonProperty("asks")]
        public decimal[][] Asks { get; set; }

        [JsonProperty("bids")]
        public decimal[][] Bids { get; set; }
    }
}

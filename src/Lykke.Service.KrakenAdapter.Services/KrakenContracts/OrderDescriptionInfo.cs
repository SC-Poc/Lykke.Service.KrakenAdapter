using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public sealed class OrderDescriptionInfo
    {
        [JsonProperty("pair")]
        public string Pair { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KrakenTradeType Type { get; set; }

        [JsonProperty("ordertype")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KrakenOrderType Ordertype { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("price2")]
        public decimal Price2 { get; set; }

        [JsonProperty("leverage")]
        public string Leverage { get; set; }

        [JsonProperty("order")]
        public string Order { get; set; }

        [JsonProperty("close")]
        public string Close { get; set; }
    }
}

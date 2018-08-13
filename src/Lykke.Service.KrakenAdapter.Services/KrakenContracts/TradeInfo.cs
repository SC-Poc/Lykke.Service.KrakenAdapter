using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public sealed class TradeInfo
    {
        /// <summary>
        /// order responsible for execution of trade
        /// </summary>
        [JsonProperty("ordertxid")]
        public object Ordertxid { get; set; }

        /// <summary>
        /// asset pair
        /// </summary>
        [JsonProperty("pair")]
        public object Pair { get; set; }

        /// <summary>
        /// unix timestamp of trade
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(KrakenFloatUnixDateTimeConverter))]
        public DateTime Time { get; set; }

        /// <summary>
        /// type of order (buy/sell)
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KrakenTradeType Type { get; set; }

        /// <summary>
        /// order type
        /// </summary>
        [JsonProperty("ordertype")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KrakenOrderType Ordertype { get; set; }

        /// <summary>
        /// average price order was executed at (quote currency)
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// total cost of order (quote currency)
        /// </summary>
        [JsonProperty("cost")]
        public decimal Cost { get; set; }

        /// <summary>
        /// total fee (quote currency)
        /// </summary>
        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        /// <summary>
        /// volume (base currency)
        /// </summary>
        [JsonProperty("vol")]
        public decimal Vol { get; set; }

        /// <summary>
        /// initial margin (quote currency)
        /// </summary>
        [JsonProperty("margin")]
        public decimal Margin { get; set; }

        /// <summary>
        /// comma delimited list of miscellaneous info
        /// </summary>
        [JsonProperty("misc")]
        public string Misc { get; set; }

        /// <summary>
        /// trade closes all or part of a position
        /// </summary>
        [JsonProperty("closing")]
        public object Closing { get; set; }

        /// <summary>
        /// amount of available trades info matching criteria
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}

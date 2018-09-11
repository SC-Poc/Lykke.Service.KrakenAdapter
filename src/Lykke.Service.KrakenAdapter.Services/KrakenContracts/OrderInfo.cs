using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public sealed class OrderInfo
    {
        [JsonProperty("refid")]
        public string Refid { get; set; }

        [JsonProperty("userref")]
        public string Userref { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KrakenOrderStatus Status { get; set; }

        [JsonProperty("opentm")]
        [JsonConverter(typeof(KrakenFloatUnixDateTimeConverter))]
        public DateTime? Opentm { get; set; }

        [JsonProperty("starttm")]
        [JsonConverter(typeof(KrakenFloatUnixDateTimeConverter))]
        public DateTime? Starttm { get; set; }

        [JsonProperty("expiretm")]
        [JsonConverter(typeof(KrakenFloatUnixDateTimeConverter))]
        public DateTime? Expiretm { get; set; }

        [JsonProperty("descr")]
        public OrderDescriptionInfo Descr { get; set; }

        [JsonProperty("vol")]
        public decimal Volume { get; set; }

        [JsonProperty("vol_exec")]
        public decimal ExecutedVolume { get; set; }

        [JsonProperty("cost")]
        public decimal TotalCost { get; set; }

        [JsonProperty("fee")]
        public decimal TotalFee { get; set; }

        [JsonProperty("price")]
        public decimal AveragePrice { get; set; }

        public IReadOnlyCollection<string> Trades { get; set; }
    }
}

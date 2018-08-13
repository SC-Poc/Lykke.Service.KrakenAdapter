using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services.Instruments
{
    public struct KrakenInstrument
    {
        public readonly string Value;

        public KrakenInstrument(string value)
        {
            Value = value.ToUpperInvariant();
        }
    }

    public sealed class KrakenInstrumentDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("altname")]
        public string AltName { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("quote")]
        public string Quote { get; set; }

        public KrakenInstrumentDefinition(string name, string altName, string @base, string quote)
        {
            Name = name;
            AltName = altName;
            Base = @base;
            Quote = quote;
        }
    }
}

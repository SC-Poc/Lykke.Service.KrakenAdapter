using System;
using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public class KrakenFloatUnixDateTimeConverter : JsonConverter<DateTime?>
    {
        public override void WriteJson(JsonWriter writer, DateTime? value, JsonSerializer serializer)
        {
            if (!value.HasValue)
            {
                writer.WriteValue(0);
                return;
            }
            var val = Math.Round((value.Value - DateTime.UnixEpoch).TotalSeconds, 4);
            writer.WriteValue(val);
        }

        public override DateTime? ReadJson(
            JsonReader reader,
            Type objectType,
            DateTime? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            double? val;

            switch (reader.TokenType)
            {
                case JsonToken.String:
                    if (double.TryParse((string)reader.Value, out var parsed))
                    {
                        val = parsed;
                    }
                    else
                    {
                        val = null;
                    }

                    break;
                case JsonToken.Float:
                    val = (double) reader.Value;
                    break;
                case JsonToken.Integer:
                    var asInt = (long)reader.Value;
                    if (asInt == 0) return null;
                    val = asInt;
                    break;
                default:
                    val = null;
                    break;
            }

            if (val == null)
            {
                return null;
            }

            return DateTime.UnixEpoch + TimeSpan.FromSeconds(val.Value);
        }
    }
}

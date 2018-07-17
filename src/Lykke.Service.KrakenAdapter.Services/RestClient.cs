using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Newtonsoft.Json;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class RestClient
    {
        private static readonly HttpClient Client = new HttpClient()
        {
            BaseAddress = new Uri("https://api.kraken.com")
        };

        public async Task<IReadOnlyDictionary<string, KrakenInstrumentDefinition>> GetInstruments()
        {
            var dict = await Get<IReadOnlyDictionary<string, KrakenInstrumentDefinition>>("/0/public/AssetPairs");

            foreach (var keyValuePair in dict)
            {
                keyValuePair.Value.Name = keyValuePair.Key;
            }

            return dict;
        }

        private static async Task<T> Get<T>(string path)
        {
            using (var msg = await Client.GetAsync(path))
            {
                return await ReadAsKrakenResponse<T>(msg);
            }
        }

        private sealed class KrakenResponse<T>
        {
            [JsonProperty("error")]
            public IReadOnlyCollection<string> Errors { get; set; }

            [JsonProperty("result")]
            public T Result { get; set; }
        }

        private static async Task<T> ReadAsKrakenResponse<T>(HttpResponseMessage msg)
        {
            msg.EnsureSuccessStatusCode();

            var response = await msg.Content.ReadAsAsync<KrakenResponse<T>>();
            if (response.Errors.Any())
            {
                throw new KrakenApiException(response.Errors);
            }

            return response.Result;
        }

        public sealed class AsksAndBids
        {
            [JsonProperty("asks")]
            public decimal[][] Asks { get; set; }

            [JsonProperty("bids")]
            public decimal[][] Bids { get; set; }
        }

        public Task<IReadOnlyDictionary<string, AsksAndBids>> GetOrderBook(KrakenInstrument instrument, int depth)
        {
            return Get<IReadOnlyDictionary<string, AsksAndBids>>(
                $"/0/public/Depth?pair={WebUtility.UrlEncode(instrument.Value)}&depth={depth}");
        }
    }

    public class KrakenApiException : Exception
    {
        public KrakenApiException(IEnumerable<string> errors)
            : base($"Error calling kraken api: {string.Join("; ", errors)}")
        {
        }
    }
}

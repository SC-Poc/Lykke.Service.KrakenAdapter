using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Server.Fails;
using Lykke.Common.Log;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Lykke.Service.KrakenAdapter.Services.KrakenContracts;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class RestClient
    {
        private readonly HttpClient _client;

        public RestClient(ILogFactory lf, ApiCredentials credentials)
        {
            var log = lf.CreateLog(this);

            var loggingHandler = new LoggingHandler(
                log,
                new HttpClientHandler(),
                "/0/public/Depth");

            var auth = new AuthenticationHandler(credentials, loggingHandler);

            _client = new HttpClient(auth)
            {
                BaseAddress = new Uri("https://api.kraken.com")
            };
        }

        public RestClient(ILogFactory logFactory) : this(logFactory, null)
        {
        }

        public async Task<IReadOnlyDictionary<string, KrakenInstrumentDefinition>> GetInstruments()
        {
            var dict = await Get<IReadOnlyDictionary<string, KrakenInstrumentDefinition>>("/0/public/AssetPairs");

            foreach (var keyValuePair in dict)
            {
                keyValuePair.Value.Name = keyValuePair.Key;
            }

            return dict;
        }

        private async Task<T> Get<T>(string path)
        {
            using (var msg = await _client.GetAsync(path))
            {
                return await ReadAsKrakenResponse<T>(msg);
            }
        }

        private async Task<T> Post<T>(string path, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var content = new FormUrlEncodedContent(parameters);

            using (var msg = await _client.PostAsync(path, content))
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

            var known = new Dictionary<string, Func<string, Exception>>
            {
                {"EOrder:Unknown order", s => new OrderNotFoundException(s)},
                {"EGeneral:Invalid arguments:volume", s => new VolumeTooSmallException(s)},
                {"EGeneral:Invalid arguments:price", s => new InvalidOrderPriceException(s)},
                {"EOrder:Insufficient funds", s => new InsufficientBalanceException(s)},
                {"EOrder:Invalid price", s => new InvalidOrderPriceException(s)},
            };

            if (!response.Errors.Any())
            {
                return response.Result;
            }

            foreach (var err in known)
            {
                if (response.Errors.Contains(err.Key))
                {
                    throw err.Value(err.Key);
                }
            }

            throw new KrakenApiException(response.Errors);

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

        public async Task<LimitOrderCreated> CreateLimitOrder(
            KrakenInstrument instrument,
            TradeType tradeType,
            decimal price,
            decimal volume)
        {
            var p = new Dictionary<string, string>
            {
                {"pair", instrument.Value},
                {"type", TradeTypeToString(tradeType)},
                {"ordertype", "limit"},
                {"price", price.ToString(CultureInfo.InvariantCulture)},
                {"volume", volume.ToString(CultureInfo.InvariantCulture)},
            };

            return await Post<LimitOrderCreated>("/0/private/AddOrder", p);
        }

        public async Task<Dictionary<string, KrakenContracts.OrderInfo>> GetOpenOrders()
        {
            var orders = await Post<Dictionary<string, Dictionary<string, KrakenContracts.OrderInfo>>>(
                "/0/private/OpenOrders");

            return orders["open"];
        }

        public async Task<JToken> GetTradesHistory()
        {
            return await Post<JToken>("/0/private/TradesHistory");
        }

        private Task<T> Post<T>(string path)
        {
            return Post<T>(path, new KeyValuePair<string, string>[0]);
        }

        private string TradeTypeToString(TradeType tradeType)
        {
            if (tradeType == TradeType.Buy) return "buy";
            if (tradeType == TradeType.Sell) return "sell";
            throw new ArgumentOutOfRangeException(nameof(tradeType), $"Not supported (only buy/sell yet)");
        }

        public async Task<OrdersCanceled> CancelOpenOrder(string txid)
        {
            var request = new Dictionary<string, string> {{"txid", txid}};

            return await Post<OrdersCanceled>("/0/private/CancelOrder", request);
        }

        public async Task<Dictionary<string, decimal>> GetBalances()
        {
            return await Post<Dictionary<string, decimal>>("/0/private/Balance");
        }

        public async Task<Dictionary<string, decimal>> GetTradeBalances()
        {
            return await Post<Dictionary<string, decimal>>("0/private/TradeBalance");
        }

        public Task<Dictionary<string, OrderInfo>> QueryOrders(string orderId)
        {
            var request = new Dictionary<string, string>
            {
                {"txid", orderId},
                {"trades", "true"}
            };
            return Post<Dictionary<string, OrderInfo>>("/0/private/QueryOrders", request);
        }
    }
}

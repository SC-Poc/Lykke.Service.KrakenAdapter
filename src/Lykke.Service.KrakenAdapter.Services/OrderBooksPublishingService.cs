using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Server.Settings;
using Lykke.Common.Log;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class OrderBooksPublishingService : IHostedService
    {
        private readonly ILogFactory _logFactory;
        private readonly KrakenOrderBookProcessingSettings _settings;
        public OrderBooksSession Session { get; private set; }

        private IDisposable _disposable;

        private readonly RestClient _restClient = new RestClient();
        private InstrumentsConverter _converter;
        private const int OrderBookDepth = 100;
        private const string KrakenSourceName = "kraken";

        private readonly ILog _log;

        public OrderBooksPublishingService(
            ILogFactory logFactory,
            KrakenOrderBookProcessingSettings settings)
        {
            _logFactory = logFactory;
            _settings = settings;
            _log = _logFactory.CreateLog(this);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Session = await CreateSession();

            _disposable = new CompositeDisposable(
                // because publisher could be disabled in settings we call this implicitly
                Session.Worker.Subscribe(),
                Session);
        }

        private async Task<OrderBooksSession> CreateSession()
        {
            _converter = new InstrumentsConverter(await _restClient.GetInstruments());

            var orderBooks = _converter
                .KrakenInstruments
                .Select(i => PullOrderBooks(i, _settings.TimeoutBetweenQueries))
                .Merge();

            return orderBooks.FromRawOrderBooks(
                _converter.LykkeInstruments.Select(x => x.Value).ToArray(),
                _settings,
                _logFactory);
        }

        private IObservable<OrderBook> PullOrderBooks(KrakenInstrument instrument, TimeSpan delay)
        {
            return Observable.Create<OrderBook>(async (obs, ct) =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var asksAndBids = await _restClient.GetOrderBook(instrument, OrderBookDepth);
                    var ob = ConvertOrderBook(instrument, asksAndBids.Values.FirstOrDefault());

                    // _log.Info($"{ob.Asset}: {ob.BestBidPrice} / {ob.BestAskPrice}");

                    obs.OnNext(ob);

                    await Task.Delay(delay, ct);
                }
            });
        }

        private OrderBook ConvertOrderBook(KrakenInstrument asset, RestClient.AsksAndBids ob)
        {
            return new OrderBook(
                KrakenSourceName,
                _converter.FromKrakenInstrument(asset).Value,
                DateTime.UtcNow,
                bids: ob.Bids.Select(x => new OrderBookItem(x[0], x[1])),
                asks: ob.Asks.Select(x => new OrderBookItem(x[0], x[1]))
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _disposable?.Dispose();
            return Task.CompletedTask;
        }
    }
}

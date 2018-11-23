using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Lykke.Service.KrakenAdapter.Services.KrakenContracts;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class OrderBooksPublishingService : IHostedService
    {
        private const int OrderBookDepth = 100;
        private const string KrakenSourceName = "kraken";

        private readonly ILogFactory _logFactory;
        private readonly RestClient _restClient;
        private readonly KrakenOrderBookProcessingSettings _settings;

        private IDisposable _disposable;
        private InstrumentsConverter _converter;

        public OrderBooksPublishingService(ILogFactory logFactory, KrakenOrderBookProcessingSettings settings)
        {
            _logFactory = logFactory;
            _restClient = new RestClient(_logFactory);
            _settings = settings;
        }

        public OrderBooksSession Session { get; private set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Session = await CreateSession();

            _disposable = new CompositeDisposable(
                // because publisher could be disabled in settings we call this implicitly
                Session.Worker.Subscribe(),
                Session);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _disposable?.Dispose();
            return Task.CompletedTask;
        }

        private async Task<OrderBooksSession> CreateSession()
        {
            _converter = new InstrumentsConverter(await _restClient.GetInstruments());

            var orderBooks = _converter
                .KrakenInstruments
                .Select(i => PullOrderBooks(i, _settings.TimeoutBetweenQueries))
                .Merge();

            return OrderBooksSession.FromRawOrderBooks(
                orderBooks,
                _settings,
                _logFactory);
        }

        private IObservable<OrderBook> PullOrderBooks(KrakenInstrument instrument, TimeSpan delay)
        {
            return Observable.Create<OrderBook>(async (orderBooks, ct) =>
            {
                while (!ct.IsCancellationRequested)
                {
                    IReadOnlyDictionary<string, AsksAndBids> asksAndBids =
                        await _restClient.GetOrderBook(instrument, OrderBookDepth);

                    var orderBook = ConvertOrderBook(instrument, asksAndBids.Values.FirstOrDefault());

                    // _log.Info($"{ob.Asset}: {ob.BestBidPrice} / {ob.BestAskPrice}");

                    orderBooks.OnNext(orderBook);

                    await Task.Delay(delay, ct);
                }
            });
        }

        private OrderBook ConvertOrderBook(KrakenInstrument asset, AsksAndBids ob)
        {
            return new OrderBook(
                KrakenSourceName,
                _converter.FromKrakenInstrument(asset).Value,
                DateTime.UtcNow,
                bids: ob.Bids.Select(x => new OrderBookItem(x[0], x[1])),
                asks: ob.Asks.Select(x => new OrderBookItem(x[0], x[1]))
            );
        }
    }
}

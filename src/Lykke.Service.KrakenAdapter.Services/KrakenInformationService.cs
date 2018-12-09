using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.Log;
using Lykke.Service.KrakenAdapter.Core.Services;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class KrakenInformationService : IHostedService
    {
        private IDisposable _worker;

        public IObservable<InstrumentsConverter> Converter { get; }

        public Task<InstrumentsConverter> LatestConverter => Converter.FirstOrDefaultAsync().ToTask();

        public KrakenInformationService(ISettingsService settingsService, ILogFactory logFactory)
        {
            var client = new RestClient(settingsService.GetApiRetrySettings(), logFactory);

            var instruments =
                Observable.Interval(TimeSpan.FromMinutes(30))
                    .StartWith(0)
                    .SelectMany(async _ => new InstrumentsConverter(await client.GetInstruments()))
                    .RetryWithBackoff(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10))
                    .ShareLatest();

            Converter = instruments;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _worker = Converter.Subscribe();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _worker?.Dispose();
            return Task.CompletedTask;
        }
    }
}

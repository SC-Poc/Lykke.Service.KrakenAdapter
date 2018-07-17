using System;
using Lykke.Common.ExchangeAdapter.Server.Settings;

namespace Lykke.Service.KrakenAdapter.Services
{
    public sealed class KrakenOrderBookProcessingSettings : OrderBookProcessingSettings
    {
        public TimeSpan TimeoutBetweenQueries { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Lykke.Service.KrakenAdapter.Services
{
    public class KrakenApiException : Exception
    {
        public KrakenApiException(IEnumerable<string> errors)
            : base($"Error calling kraken api: {string.Join("; ", errors)}")
        {
        }
    }
}
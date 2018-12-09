using System;
using System.Net;

namespace Lykke.Service.KrakenAdapter.Services
{
    public class KrakenApiRequestException : Exception
    {
        public KrakenApiRequestException(HttpStatusCode httpStatusCode, string message)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        public HttpStatusCode HttpStatusCode { get; }
    }
}

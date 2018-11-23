using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter;

namespace Lykke.Service.KrakenAdapter.Services
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly ApiCredentials _credentials;

        public AuthenticationHandler(ApiCredentials credentials, HttpMessageHandler next)
            : base(next)
        {
            _credentials = credentials;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_credentials != null
                && request.Method == HttpMethod.Post
                && request.Content is FormUrlEncodedContent urlEncoded)
            {
                return await EpochNonce.Lock(_credentials.ApiKey, async nonce =>
                    {
                        string strNonce = nonce.ToString(CultureInfo.InvariantCulture);
                        request.Content = await AddNonce(urlEncoded, strNonce);

                        request.Content.Headers.Add("API-Key", _credentials.ApiKey);
                        request.Content.Headers.Add("API-Sign", _credentials.Sign(
                            strNonce,
                            await request.Content.ReadAsStringAsync(),
                            request.RequestUri.PathAndQuery));

                        return await base.SendAsync(request, cancellationToken);
                    },
                    LockKind.EpochMilliseconds);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private static async Task<HttpContent> AddNonce(FormUrlEncodedContent urlEncoded, string strNonce)
        {
            var list = new List<KeyValuePair<string, string>> {KeyValuePair.Create("nonce", strNonce)};

            NameValueCollection parameters = await urlEncoded.ReadAsFormDataAsync();

            for (int i = 0; i < parameters.Count; i++)
            {
                string[] strings = parameters.GetValues(i);

                string key = parameters.GetKey(i);

                if (strings?.Length != 1)
                    throw new InvalidOperationException($"For key {key} expected only one value");

                list.Add(KeyValuePair.Create(key, strings[0]));
            }

            return new FormUrlEncodedContent(list);
        }
    }
}

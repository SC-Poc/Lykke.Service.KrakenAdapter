using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter;

namespace Lykke.Service.KrakenAdapter.Services
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly ApiCredentials _credentials;

        public AuthenticationHandler(ApiCredentials credentials, HttpMessageHandler next) : base(next)
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
                        var strNonce = nonce.ToString(CultureInfo.InvariantCulture);
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
            else return await base.SendAsync(request, cancellationToken);
        }

        private async Task<HttpContent> AddNonce(FormUrlEncodedContent urlEncoded, string strNonce)
        {
            var list = new List<KeyValuePair<string, string>> {KeyValuePair.Create("nonce", strNonce)};

            var nvc = await urlEncoded.ReadAsFormDataAsync();

            for (int i = 0; i < nvc.Count; i++)
            {
                var strings = nvc.GetValues(i);
                var key = nvc.GetKey(i);

                if (strings.Length != 1)
                {
                    throw new InvalidOperationException($"For key {key} expected only one value");
                }



                list.Add(KeyValuePair.Create(key, strings[0]));
            }

            return new FormUrlEncodedContent(list);
        }
    }

    public class ApiCredentials
    {
        public readonly string ApiKey;
        private readonly byte[] _apiSecret;

        public ApiCredentials(string apiKey, string base64Secret)
        {
            ApiKey = apiKey;
            _apiSecret = Convert.FromBase64String(base64Secret);
        }

        public string Sign(string nonce, string postData, string uriPath)
        {
            using (var sha256 = new SHA256Managed())
            {
                var np = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{nonce}{postData}"));

                using (var ms = new MemoryStream())
                {
                    var pathBytes = Encoding.UTF8.GetBytes(uriPath);
                    ms.Write(pathBytes, 0, pathBytes.Length);

                    ms.Write(np, 0, np.Length);
                    ms.Flush();

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var sha512 = new HMACSHA512(_apiSecret))
                    {
                        return Convert.ToBase64String(sha512.ComputeHash(ms));
                    }
                }
            }
        }
    }
}

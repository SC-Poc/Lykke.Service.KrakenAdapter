using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lykke.Service.KrakenAdapter.Services
{
    public class ApiCredentials
    {
        private readonly byte[] _apiSecret;

        public ApiCredentials(string apiKey, string base64Secret)
        {
            ApiKey = apiKey;
            _apiSecret = Convert.FromBase64String(base64Secret);
        }

        public string ApiKey { get; }

        public string Sign(string nonce, string postData, string uriPath)
        {
            using (var sha256 = new SHA256Managed())
            {
                byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{nonce}{postData}"));

                using (var stream = new MemoryStream())
                {
                    byte[] pathBytes = Encoding.UTF8.GetBytes(uriPath);

                    stream.Write(pathBytes, 0, pathBytes.Length);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    using (var sha512 = new HMACSHA512(_apiSecret))
                        return Convert.ToBase64String(sha512.ComputeHash(stream));
                }
            }
        }
    }
}

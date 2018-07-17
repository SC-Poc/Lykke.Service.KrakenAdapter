using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Lykke.Service.KrakenAdapter.Tests
{
    public class rest_client_tests
    {
        static rest_client_tests()
        {
            Client = new RestClient();
            Converter = new InstrumentsConverter(Client.GetInstruments().Result);
        }

        private static readonly RestClient Client;
        private static readonly InstrumentsConverter Converter;

        [Test]
        public void convert_samples()
        {
            Assert.AreEqual("BTCUSD", Converter.FromKrakenInstrument(new KrakenInstrument("XXBTZUSD")).Value);
        }

        [Test]
        public async Task get_orderbook()
        {
            var ob = await Client.GetOrderBook(new KrakenInstrument("XXBTZUSD"), 10);
            Console.WriteLine(JsonConvert.SerializeObject(ob));
        }
    }
}

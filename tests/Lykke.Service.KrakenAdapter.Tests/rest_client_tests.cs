using System;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Lykke.Service.KrakenAdapter.Tests
{
    [Ignore("manual")]
    public class rest_client_tests
    {
        static rest_client_tests()
        {

            _lf = LogFactory.Create().AddUnbufferedConsole();

            Client = new RestClient(_lf, new ApiCredentials("", ""));
            Converter = new InstrumentsConverter(Client.GetInstruments().Result);
        }

        public rest_client_tests()
        {
            _log = _lf.CreateLog(this);
        }

        private static readonly RestClient Client;
        private static readonly InstrumentsConverter Converter;
        private static readonly ILogFactory _lf;
        private ILog _log;

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

        [Test]
        public async Task get_balances()
        {
            var ob = await Client.GetBalances();
            Console.WriteLine(ob);
        }

        [Test]
        public async Task get_open_orders()
        {
            var ob = await Client.GetOpenOrders();
            Console.WriteLine(JsonConvert.SerializeObject(ob));
        }

        [Test]
        public async Task create_limit_order()
        {
            var ob = await Client.CreateLimitOrder(
                new KrakenInstrument("XXBTZUSD"),
                TradeType.Sell,
                6900M,
                0.002M);

            Console.WriteLine(ob);
        }

        [Test]
        public async Task cancel_open_order()
        {
            var ob = await Client.CancelOpenOrder("O3ZH24-UVNCU-FRCUDZ");

            Console.WriteLine(ob);
        }

        [Test]
        public async Task trade_history()
        {
            var ob = await Client.GetTradesHistory();

            Console.WriteLine(ob);
        }
    }
}

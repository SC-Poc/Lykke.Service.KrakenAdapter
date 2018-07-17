using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Service.KrakenAdapter.Services;

namespace Lykke.Service.KrakenAdapter.Controllers
{
    public sealed class OrderBooksController : OrderBookControllerBase
    {
        protected override OrderBooksSession Session { get; }

        public OrderBooksController(OrderBooksPublishingService service)
        {
            Session = service.Session;
        }
    }
}

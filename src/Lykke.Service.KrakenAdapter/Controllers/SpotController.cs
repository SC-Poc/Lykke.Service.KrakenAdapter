using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Server.Fails;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Common.Log;
using Lykke.Service.KrakenAdapter.Services;
using Lykke.Service.KrakenAdapter.Services.Instruments;
using Lykke.Service.KrakenAdapter.Services.KrakenContracts;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.KrakenAdapter.Controllers
{
    public class SpotController : SpotControllerBase<RestClient>
    {
        private readonly KrakenInformationService _informationService;
        private readonly ILog _log;

        public SpotController(
            ILogFactory lf,
            KrakenInformationService informationService)
        {
            _log = lf.CreateLog(this);
            _informationService = informationService;
        }

        public override async Task<OrderIdResponse> CreateLimitOrderAsync([FromBody] LimitOrderRequest request)
        {
            var converter = await _informationService.LatestConverter;

            var result = await Api.CreateLimitOrder(
                converter.FromLykkeInstrument(new LykkeInstrument(request.Instrument)),
                request.TradeType,
                request.Price,
                request.Volume);

            if (result.Txid.Length != 1)
            {
                _log.Warning($"Expected single txid, got: [{string.Join("; ", result.Txid)}]");
            }

            return new OrderIdResponse {OrderId = result.Txid[0]};
        }

        public override async Task<CancelLimitOrderResponse> CancelLimitOrderAsync(CancelLimitOrderRequest request)
        {
            var response = await Api.CancelOpenOrder(request.OrderId);

            if (response.Count != 1)
            {
                _log.Warning($"Expected single order canceled, but count = {response.Count}");
            }

            return new CancelLimitOrderResponse {OrderId = request.OrderId};
        }

        public override async Task<GetLimitOrdersResponse> GetLimitOrdersAsync()
        {
            var orders = await Api.GetOpenOrders();
            var converter = await _informationService.LatestConverter;

            return new GetLimitOrdersResponse
            {
                Orders = orders.Select(x => GetLimitFromExchangeOrder(x.Key, x.Value, converter))
                    .Where(x => x != null)
                    .ToArray()
            };
        }

        private OrderModel GetLimitFromExchangeOrder(
            string txid,
            OrderInfo order,
            InstrumentsConverter converter)
        {
            if (!TryConvert(order.Descr.Type, out var krakenTradeType)) return null;

            if (!TryGetOrderStatus(order, out var executionStatus)) return null;

            if (order.Descr.Ordertype != KrakenOrderType.Limit)
            {
                return null;
            }

            return new OrderModel
            {
                TradeType = krakenTradeType,
                Price = order.Descr.Price,
                OriginalVolume = order.Volume,
                ExecutedVolume = order.ExecutedVolume,
                AvgExecutionPrice = order.AveragePrice,
                ExecutionStatus = executionStatus,
                Id = txid,
                RemainingAmount = order.Volume - order.ExecutedVolume,
                Symbol = converter.FromKrakenInstrument(new KrakenInstrument(order.Descr.Pair)).Value,
                Timestamp = order.Opentm ?? DateTime.UtcNow
            };
        }

        private bool TryGetOrderStatus(OrderInfo order, out OrderStatus tradeType)
        {
            var canceledStatus = order.ExecutedVolume == 0 ? OrderStatus.Canceled : OrderStatus.Fill;

            switch (order.Status)
            {
                case KrakenOrderStatus.Pending:
                    tradeType = OrderStatus.Active;
                    break;
                case KrakenOrderStatus.Open:
                    tradeType = OrderStatus.Active;
                    break;
                case KrakenOrderStatus.Closed:
                    tradeType = canceledStatus;
                    break;
                case KrakenOrderStatus.Canceled:
                    tradeType = canceledStatus;
                    break;
                case KrakenOrderStatus.Expired:
                    tradeType = canceledStatus;
                    break;
                default:
                    tradeType = default;
                    return false;
            }

            return true;
        }

        private static bool TryConvert(KrakenTradeType krakenOrderType, out TradeType tradeType)
        {
            switch (krakenOrderType)
            {
                case KrakenTradeType.Buy:
                    tradeType = TradeType.Buy;
                    break;
                case KrakenTradeType.Sell:
                    tradeType = TradeType.Sell;
                    break;
                default:
                    tradeType = default;
                    return false;
            }

            return true;
        }

        public override async Task<GetWalletsResponse> GetWalletBalancesAsync()
        {
            var balances = await Api.GetBalances();

            var converter = await _informationService.LatestConverter;

            return new GetWalletsResponse
            {
                Wallets = balances.Select(x => new WalletBalanceModel
                    {
                        Asset = converter.FromKrakenInstrument(new KrakenInstrument(x.Key)).Value,
                        Balance = x.Value,
                        Reserved = 0
                    })
                    .ToArray()
            };
        }

        public override async Task<OrderModel> LimitOrderStatusAsync(string orderId)
        {
            var orders = await Api.QueryOrders(orderId);

            if (!orders.TryGetValue(orderId, out var oi)) throw new OrderNotFoundException();
            var order = GetLimitFromExchangeOrder(orderId, oi, await _informationService.LatestConverter);

            if (order == null) throw new OrderNotFoundException();

            return order;
        }
    }
}

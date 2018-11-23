using System;
using System.Collections.Generic;
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
            ILogFactory logFactory,
            KrakenInformationService informationService)
        {
            _log = logFactory.CreateLog(this);
            _informationService = informationService;
        }

        public override async Task<OrderIdResponse> CreateLimitOrderAsync([FromBody] LimitOrderRequest request)
        {
            InstrumentsConverter instrumentsConverter = await _informationService.LatestConverter;

            var result = await Api.CreateLimitOrder(
                instrumentsConverter.FromLykkeInstrument(new LykkeInstrument(request.Instrument)),
                request.TradeType,
                request.Price,
                request.Volume);

            if (result.Txid.Length != 1)
                _log.Warning($"Expected single txid, got: [{string.Join("; ", result.Txid)}]");

            return new OrderIdResponse {OrderId = result.Txid[0]};
        }

        public override async Task<CancelLimitOrderResponse> CancelLimitOrderAsync(
            [FromBody] CancelLimitOrderRequest request)
        {
            OrdersCanceled response = await Api.CancelOpenOrder(request.OrderId);

            if (response.Count != 1)
                _log.Warning($"Expected single order canceled, but count = {response.Count}");

            return new CancelLimitOrderResponse {OrderId = request.OrderId};
        }

        public override async Task<GetLimitOrdersResponse> GetLimitOrdersAsync()
        {
            IReadOnlyDictionary<string, OrderInfo> orders = await Api.GetOpenOrders();
            
            InstrumentsConverter instrumentsConverter = await _informationService.LatestConverter;

            return new GetLimitOrdersResponse
            {
                Orders = orders.Select(x => GetLimitFromExchangeOrder(x.Key, x.Value, instrumentsConverter))
                    .Where(x => x != null)
                    .ToArray()
            };
        }

        public override async Task<GetWalletsResponse> GetWalletBalancesAsync()
        {
            IReadOnlyDictionary<string, decimal> balances = await Api.GetBalances();

            InstrumentsConverter instrumentsConverter = await _informationService.LatestConverter;

            return new GetWalletsResponse
            {
                Wallets = balances.Select(x => new WalletBalanceModel
                    {
                        Asset = instrumentsConverter.FromKrakenCurrency(x.Key),
                        Balance = x.Value
                    })
                    .ToArray()
            };
        }

        public override async Task<OrderModel> LimitOrderStatusAsync(string orderId)
        {
            IReadOnlyDictionary<string, OrderInfo> orders = await Api.QueryOrders(orderId);

            if (!orders.TryGetValue(orderId, out OrderInfo orderInfo))
                throw new OrderNotFoundException();
            
            OrderModel order = GetLimitFromExchangeOrder(orderId, orderInfo, await _informationService.LatestConverter);

            if (order == null)
                throw new OrderNotFoundException();

            return order;
        }

        private OrderModel GetLimitFromExchangeOrder(string txid, OrderInfo order, InstrumentsConverter converter)
        {
            if (!TryConvert(order.Descr.Type, out TradeType krakenTradeType))
                return null;

            if (!TryGetOrderStatus(order, out OrderStatus executionStatus))
                return null;

            if (order.Descr.OrderType != KrakenOrderType.Limit)
                return null;

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

        private static bool TryGetOrderStatus(OrderInfo order, out OrderStatus tradeType)
        {
            OrderStatus canceledStatus = order.ExecutedVolume == 0
                ? OrderStatus.Canceled
                : OrderStatus.Fill;

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
    }
}

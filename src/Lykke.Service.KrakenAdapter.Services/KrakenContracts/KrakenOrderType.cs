using System.Runtime.Serialization;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public enum KrakenOrderType
    {
        // market
        [EnumMember(Value = "market")]
        Market,

        // price = limit price
        [EnumMember(Value = "limit")]
        Limit,

        // price = stop loss price
        [EnumMember(Value = "stop-loss")]
        StopLoss,

        // price = take profit price
        [EnumMember(Value = "take-profit")]
        TakeProfit,

        // price = stop loss price, price2 = take profit price
        [EnumMember(Value = "stop-loss-profit")]
        StopLossProfit,

        // price = stop loss price, price2 = take profit price
        [EnumMember(Value = "stop-loss-profit-limit")]
        StopLossProfitLimit,

        // price = stop loss trigger price, price2 = triggered limit price
        [EnumMember(Value = "stop-loss-limit")]
        StopLossLimit,

        // price = take profit trigger price, price2 = triggered limit price
        [EnumMember(Value = "take-profit-limit")]
        TakeProfitLimit,

        // price = trailing stop offset
        [EnumMember(Value = "trailing-stop")]
        TrailingStop,

        // price = trailing stop offset, price2 = triggered limit offset
        [EnumMember(Value = "trailing-stop-limit")]
        TrailingStopLimit,

        // price = stop loss price, price2 = limit price
        [EnumMember(Value = "stop-loss-and-limit")]
        StopLossAndLimit,

        // settle-position
        [EnumMember(Value = "settle-position")]
        SettlePosition
    }
}

using System.Runtime.Serialization;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public enum KrakenTradeType
    {
        [EnumMember(Value = "buy")]
        Buy,
        
        [EnumMember(Value = "sell")]
        Sell
    }
}

using System.Runtime.Serialization;

namespace Lykke.Service.KrakenAdapter.Services.KrakenContracts
{
    public enum KrakenOrderStatus
    {
        [EnumMember(Value = "pending")] Pending,
        [EnumMember(Value = "open")] Open,
        [EnumMember(Value = "closed")] Closed,
        [EnumMember(Value = "canceled")] Canceled,
        [EnumMember(Value = "expired")] Expired,
    }
}

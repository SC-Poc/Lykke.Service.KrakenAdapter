namespace Lykke.Service.KrakenAdapter.Core.Domain
{
    /// <summary>
    /// Contains kraken API retry settings.
    /// </summary>
    public class ApiRetrySettings
    {
        /// <summary>
        /// The number of retry attempts.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The delay in milliseconds between retry attempts.
        /// </summary>
        public int Delay { get; set; }
    }
}

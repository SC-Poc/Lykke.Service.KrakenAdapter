namespace Lykke.Service.KrakenAdapter.Services.Instruments
{
    public struct KrakenInstrument
    {
        public KrakenInstrument(string value)
        {
            Value = value.ToUpperInvariant();
        }

        public string Value { get; }
    }
}

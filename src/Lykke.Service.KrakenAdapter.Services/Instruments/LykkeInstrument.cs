namespace Lykke.Service.KrakenAdapter.Services.Instruments
{
    public struct LykkeInstrument
    {
        public LykkeInstrument(string value)
        {
            Value = value.ToUpperInvariant();
        }

        public string Value { get; }
    }
}

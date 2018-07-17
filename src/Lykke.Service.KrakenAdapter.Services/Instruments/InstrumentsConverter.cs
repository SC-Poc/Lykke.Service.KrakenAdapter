using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.KrakenAdapter.Services.Instruments
{
    public sealed class InstrumentsConverter
    {
        private readonly Dictionary<string, KrakenInstrumentDefinition> _knownInstruments;
        private readonly Dictionary<string, LykkeInstrument> _lykkeInstruments;
        public readonly IReadOnlyCollection<KrakenInstrument> KrakenInstruments;
        public readonly IReadOnlyCollection<LykkeInstrument> LykkeInstruments;

        public InstrumentsConverter(IReadOnlyDictionary<string, KrakenInstrumentDefinition> knownInstruments)
        {
            _knownInstruments =
                new Dictionary<string, KrakenInstrumentDefinition>(
                    knownInstruments,
                    StringComparer.InvariantCultureIgnoreCase);

            KrakenInstruments = _knownInstruments
                .Where(x => !x.Value.Name.EndsWith(".d"))
                .Select(x => new KrakenInstrument(x.Value.Name)).ToHashSet();

            _lykkeInstruments =
                new Dictionary<string, LykkeInstrument>(
                    knownInstruments.ToDictionary(
                        x => x.Value.Name,
                        x => CreateLykkeInstrument(x.Value)),
                    StringComparer.InvariantCultureIgnoreCase);

            LykkeInstruments = _lykkeInstruments.Values.ToHashSet();
        }

        private static readonly IReadOnlyDictionary<string, string> Replaces = new Dictionary<string, string>
        {
            { "XBT", "BTC" },
        };

        private static LykkeInstrument CreateLykkeInstrument(KrakenInstrumentDefinition definition)
        {
            var suffix = definition.Name.EndsWith(".d") ? ".d": "";

            return new LykkeInstrument($"{Simplify(definition.Base)}{Simplify(definition.Quote)}{suffix}");
        }

        private static string Simplify(string currency)
        {
            if (string.IsNullOrEmpty(currency)) throw new ArgumentException("Currency is empty", nameof(currency));

            if (currency[0] == 'X' || currency[0] == 'Z')
            {
                currency = currency.Substring(1);
            }

            return Replaces.TryGetValue(currency, out var lykke) ? lykke : currency;
        }

        public LykkeInstrument FromKrakenInstrument(KrakenInstrument instrument)
        {
            if (!_lykkeInstruments.TryGetValue(instrument.Value, out var li))
            {
                throw new ArgumentException("Unknown instrument", nameof(instrument));
            }

            return li;
        }
    }
}

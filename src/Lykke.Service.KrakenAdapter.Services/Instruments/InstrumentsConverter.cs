using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Common.ExchangeAdapter.Server.Fails;

namespace Lykke.Service.KrakenAdapter.Services.Instruments
{
    public sealed class InstrumentsConverter
    {
        public InstrumentsConverter(IReadOnlyDictionary<string, KrakenInstrumentDefinition> knownInstruments)
        {
            var wholeLengthNames = knownInstruments
                .Where(x => !x.Value.Name.EndsWith(".d"))
                .Select(x => (new KrakenInstrument(x.Value.Name), x.Value))
                .ToDictionary(x => x.Item1, x => x.Item2);

            _byKrakenInstrument =
                WithAltNames(wholeLengthNames)
                    .GroupBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => CreateLykkeInstrument(x.First().Value));

            _byLykkeInstrument =
                wholeLengthNames
                    .Select(x => (x.Key, CreateLykkeInstrument(x.Value)))
                    .GroupBy(x => x.Item2)
                    .ToDictionary(x => x.Key, x => x.First().Item1);

            KrakenInstruments = new HashSet<KrakenInstrument>(wholeLengthNames.Select(x => x.Key));
        }

        public IReadOnlyCollection<KrakenInstrument> KrakenInstruments { get; }

        private static IEnumerable<KeyValuePair<KrakenInstrument, KrakenInstrumentDefinition>> WithAltNames(
            Dictionary<KrakenInstrument, KrakenInstrumentDefinition> knownInstruments)
        {
            foreach (var pair in knownInstruments)
            {
                yield return pair;

                if (!string.IsNullOrEmpty(pair.Value.AltName))
                    yield return KeyValuePair.Create(
                        new KrakenInstrument(pair.Value.AltName),
                        pair.Value);
            }
        }

        private static readonly IReadOnlyDictionary<string, string> Replaces = new Dictionary<string, string>
        {
            { "XBT", "BTC" },
        };

        private readonly Dictionary<LykkeInstrument, KrakenInstrument> _byLykkeInstrument;
        private readonly Dictionary<KrakenInstrument, LykkeInstrument> _byKrakenInstrument;

        private static LykkeInstrument CreateLykkeInstrument(KrakenInstrumentDefinition definition)
        {
            return new LykkeInstrument($"{Rename(Simplify(definition.Base))}" +
                                       $"{Rename(Simplify(definition.Quote))}");
        }

        public string FromKrakenCurrency(string krakenCurrency)
        {
            return Rename(Simplify(krakenCurrency));
        }

        public KrakenInstrument FromLykkeInstrument(LykkeInstrument lykke)
        {
            if (!_byLykkeInstrument.TryGetValue(lykke, out var ki))
            {
                throw new InvalidInstrumentException($"Unknown lykke instrument: {lykke.Value}");
            }
            return ki;
        }

        private static string Simplify(string currency)
        {
            if (string.IsNullOrEmpty(currency)) throw new ArgumentException("Currency is empty", nameof(currency));

            if (currency[0] == 'X' || currency[0] == 'Z')
            {
                return currency.Substring(1);
            }

            return currency;
        }

        private static string Rename(string currency)
        {
            return Replaces.TryGetValue(currency, out var lykke) ? lykke : currency;
        }

        public LykkeInstrument FromKrakenInstrument(KrakenInstrument kraken)
        {
            if (!_byKrakenInstrument.TryGetValue(kraken, out var li))
            {
                throw new InvalidInstrumentException($"Unknown instrument: {kraken.Value}");
            }

            return li;
        }
    }
}

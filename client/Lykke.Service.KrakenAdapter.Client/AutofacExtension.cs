using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;

namespace Lykke.Service.KrakenAdapter.Client
{
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers <see cref="IKrakenAdapterClient"/> in Autofac container using <see cref="KrakenAdapterServiceClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">KrakenAdapter client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.</param>
        public static void RegisterKrakenAdapterClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] KrakenAdapterServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(KrakenAdapterServiceClientSettings.ServiceUrl));

            builder.RegisterClient<IKrakenAdapterClient>(settings?.ServiceUrl, builderConfigure);
        }
    }
}

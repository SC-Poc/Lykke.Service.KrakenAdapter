using Lykke.Service.KrakenAdapter.Core.Domain;
using Lykke.Service.KrakenAdapter.Core.Services;

namespace Lykke.Service.KrakenAdapter.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApiRetrySettings _apiRetrySettings;

        public SettingsService(ApiRetrySettings apiRetrySettings)
        {
            _apiRetrySettings = apiRetrySettings;
        }

        public ApiRetrySettings GetApiRetrySettings()
            => _apiRetrySettings;
    }
}

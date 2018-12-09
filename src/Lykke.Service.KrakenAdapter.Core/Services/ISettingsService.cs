using Lykke.Service.KrakenAdapter.Core.Domain;

namespace Lykke.Service.KrakenAdapter.Core.Services
{
    public interface ISettingsService
    {
        ApiRetrySettings GetApiRetrySettings();
    }
}

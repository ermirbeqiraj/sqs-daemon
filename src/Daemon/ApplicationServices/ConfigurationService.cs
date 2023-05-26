using Daemon.ApplicationModels;
using Infrastructure.HttpService;

namespace Daemon.ApplicationServices;
public class ConfigurationService : IConfigurationService
{
    private readonly IApiService _apiService;

    public ConfigurationService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiSettings> GetConfigurations()
    {
        var result = await _apiService.GetConfiguration() ?? throw new ArgumentNullException("Couldn't find configurations");
        var settings = new ApiSettings(result.QueueUrl!, result.ApiMaxConcurrency, result.VisibilityTimeout, result.ErrorVisibilityTimeout);

        _ = settings.ConfigurationSetupGuard();

        return settings!;
    }
}

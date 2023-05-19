using Infrastructure.HttpService.Dto;

namespace Infrastructure.HttpService;

public interface IApiService
{
    Task Request(string payload);
    Task<ApiSettingsResponse?> GetConfiguration();
}

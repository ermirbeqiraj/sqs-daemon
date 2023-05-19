using System.Text;
using System.Text.Json;
using Infrastructure.HttpService.Dto;

namespace Infrastructure.HttpService;
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Request(string payload)
    {
        var stringContent = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/", stringContent);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ApiSettingsResponse?> GetConfiguration()
    {
        var request = await _httpClient.GetAsync("configuration");
        request.EnsureSuccessStatusCode();

        var content = await request.Content.ReadAsStringAsync();
        var apiSettings = JsonSerializer.Deserialize<ApiSettingsResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return apiSettings;
    }
}

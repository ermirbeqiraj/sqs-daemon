using Infrastructure.HttpService.Dto;
using Infrastructure.HttpService;
using System.Net.Mime;
using System.Text.Json;
using RichardSzalay.MockHttp;

namespace Daemon.Test.Unit;

public class ApiServiceTests
{
    [Fact]
    public async Task GetConfiguration_WhenGivenPascalCase_MapsSuccessfully()
    {
        var expectedResponse = PreparedResponse();

        var serializedResponse = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false
        });

        var httpMsgHandler = new MockHttpMessageHandler();
        _ = httpMsgHandler.When("http://localhost/configuration")
                                                .Respond(MediaTypeNames.Application.Json, serializedResponse);

        var httpClient = httpMsgHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");

        var sut = new ApiService(httpClient);
        var result = await sut.GetConfiguration();

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.QueueUrl, result!.QueueUrl);
        Assert.Equal(expectedResponse.ApiMaxConcurrency, result.ApiMaxConcurrency);
        Assert.Equal(expectedResponse.VisibilityTimeout, result.VisibilityTimeout);
        Assert.Equal(expectedResponse.ErrorVisibilityTimeout, result.ErrorVisibilityTimeout);
    }

    [Fact]
    public async Task GetConfiguration_WhenGivenCammelCase_MapsSuccessfully()
    {
        var expectedResponse = PreparedResponse();
        var serializedResponse = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var httpMsgHandler = new MockHttpMessageHandler();
        _ = httpMsgHandler.When("http://localhost/configuration")
                                                .Respond(MediaTypeNames.Application.Json, serializedResponse);

        var httpClient = httpMsgHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");

        var sut = new ApiService(httpClient);
        var result = await sut.GetConfiguration();

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.QueueUrl, result!.QueueUrl);
        Assert.Equal(expectedResponse.ApiMaxConcurrency, result.ApiMaxConcurrency);
        Assert.Equal(expectedResponse.VisibilityTimeout, result.VisibilityTimeout);
        Assert.Equal(expectedResponse.ErrorVisibilityTimeout, result.ErrorVisibilityTimeout);
    }

    [Fact]
    public async Task Request_WhenNotSuccessful_Throws()
    {
        var httpMsgHandler = new MockHttpMessageHandler();
        _ = httpMsgHandler.When(HttpMethod.Post, "/")
                            .Respond(System.Net.HttpStatusCode.InternalServerError);

        var httpClient = httpMsgHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");

        var sut = new ApiService(httpClient);
        var result = () => sut.Request("{}");

        await Assert.ThrowsAnyAsync<HttpRequestException>(result);
    }

    private static ApiSettingsResponse PreparedResponse()
    {
        return new ApiSettingsResponse
        {
            QueueUrl = "https://the-queue-name",
            ApiMaxConcurrency = 10,
            VisibilityTimeout = 60,
            ErrorVisibilityTimeout = 10
        };
    }
}

using Daemon.ApplicationServices;
using Infrastructure.HttpService;
using Infrastructure.HttpService.Dto;
using Moq;

namespace Daemon.Test.Unit;

public class ConfigurationServiceTest
{
    [Fact]
    public async Task GetConfigurations_WhenConfiguNull_Throws()
    {
        var mock = new Mock<IApiService>();

        ApiSettingsResponse? response = null;
        mock.Setup(cnf => cnf.GetConfiguration().Result).Returns(response);

        var configService = new ConfigurationService(mock.Object);
        var act = () => configService.GetConfigurations();

        await Assert.ThrowsAsync<ArgumentNullException>(() => act());
    }

    [Fact]
    public async Task GetConfigurations_WhenQueueEmpty_Throws()
    {
        var mock = new Mock<IApiService>();

        ApiSettingsResponse? response = new() { QueueUrl = string.Empty };
        mock.Setup(cnf => cnf.GetConfiguration().Result).Returns(response);

        var configService = new ConfigurationService(mock.Object);
        var actWithEmptyQueue = () => configService.GetConfigurations();

        await Assert.ThrowsAsync<ArgumentNullException>(() => actWithEmptyQueue());
    }

    [Fact]
    public async Task GetConfigurations_WhenValuesOutOfange_Throws()
    {
        var mock = new Mock<IApiService>();

        ApiSettingsResponse? response = new() { QueueUrl = "not-empty", ApiMaxConcurrency = 0, VisibilityTimeout = 0, ErrorVisibilityTimeout = -1 };
        mock.Setup(cnf => cnf.GetConfiguration().Result).Returns(response);

        var configService = new ConfigurationService(mock.Object);
        var actWithEmptyQueue = () => configService.GetConfigurations();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => actWithEmptyQueue());
    }
}
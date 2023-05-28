using Daemon.ApplicationModels;
using Daemon.ApplicationServices;
using Daemon.Test.Unit.Fakes;
using Daemon.Workers;
using Infrastructure.HttpService;
using Infrastructure.HttpService.Dto;
using Microsoft.Extensions.Logging;
using Moq;

namespace Daemon.Test.Unit;
public class BackgroundServiceTests
{
    [Fact]
    public async Task Execute_WhenGivenEnoughTime_QueueIsDrained()
    {
        var apiResponse = SampleResponse(10);

        var configResult = new ApiSettings(apiResponse.QueueUrl!, apiResponse.ApiMaxConcurrency, apiResponse.VisibilityTimeout, apiResponse.ErrorVisibilityTimeout);

        var apiServiceMoq = new Mock<IApiService>();
        apiServiceMoq.Setup(x => x.GetConfiguration())
                        .ReturnsAsync(apiResponse);

        apiServiceMoq.Setup(x => x.Request(It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

        var iconfigMoq = new Mock<IConfigurationService>();
        iconfigMoq.Setup(x => x.GetConfigurations())
                    .ReturnsAsync(configResult);

        var queueService = new QueueServiceFake(12);
        var mock = new Mock<ILogger<ExecutorService>>();

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(20_000);

        var service = new ExecutorService(queueService, apiServiceMoq.Object, iconfigMoq.Object, mock.Object);
        await service.StartAsync(cancellationTokenSource.Token);

        while (!cancellationTokenSource.IsCancellationRequested && queueService.Queue.Any())
        {
            await Task.Delay(500);
        }

        Assert.Empty(queueService.Queue);
    }

    [Fact]
    public void GetAvailableSlots_WhenGivenMoreThenCapacity_ReturnsMaxAcceptedBySQS()
    {
        var moreThenCapacity = Constants.HardLimits.SQS_MAX_NUMBER_OF_MESSAGES + 5;

        var availableSlots = ExecutorService.GetAvailableSlots(moreThenCapacity);

        Assert.Equal(Constants.HardLimits.SQS_MAX_NUMBER_OF_MESSAGES, availableSlots);
    }

    [Fact]
    public void GetAvailableSlots_WhenGivenLessThenCapacity_ReturnsFullNumber()
    {
        var lessThenCapacity = Constants.HardLimits.SQS_MAX_NUMBER_OF_MESSAGES - 5;

        var availableSlots = ExecutorService.GetAvailableSlots(lessThenCapacity);

        Assert.Equal(lessThenCapacity, availableSlots);
    }

    private static ApiSettingsResponse SampleResponse(int concurrency)
    {
        return new ApiSettingsResponse
        {
            ApiMaxConcurrency = concurrency,
            ErrorVisibilityTimeout = 60,
            QueueUrl = "http://queue-url",
            VisibilityTimeout = 300
        };
    }
}

using Daemon.ApplicationServices;
using Infrastructure.HttpService;
using Infrastructure.QueueService;

namespace Daemon.Workers;

public class ExecutorService : BackgroundService
{
    private readonly IQueueService _queueService;
    private readonly IApiService _apiService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ExecutorService> _logger;

    public ExecutorService(IQueueService queueService, IApiService apiService, IConfigurationService configurationService, ILogger<ExecutorService> logger)
    {
        _queueService = queueService;
        _apiService = apiService;
        _configurationService = configurationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int maxSqsMsgs = 10;
        var config = await _configurationService.GetConfigurations();

        var semaphoreSlim = new SemaphoreSlim(config.ApiMaxConcurrency, config.ApiMaxConcurrency);

        while (!stoppingToken.IsCancellationRequested)
        {
            var slotsAvailable = semaphoreSlim.CurrentCount <= maxSqsMsgs ? semaphoreSlim.CurrentCount : maxSqsMsgs;

            while (slotsAvailable == 0)
            {
                await Task.Delay(1_000, stoppingToken);
                slotsAvailable = semaphoreSlim.CurrentCount <= maxSqsMsgs ? semaphoreSlim.CurrentCount : maxSqsMsgs;
            }


            var messages = await _queueService.GetMessages(config.QueueUrl,
                                                            slotsAvailable,
                                                            config.VisibilityTimeout,
                                                            stoppingToken);

            foreach (var item in messages)
            {
                await semaphoreSlim.WaitAsync(stoppingToken);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _apiService.Request(item.MessageBody);
                        await _queueService.DeleteMessage(config.QueueUrl, item.MessageHandler);
                    }
                    catch (Exception ex)
                    {
                        await _queueService.ChangeMessageVisibility(config.QueueUrl, item.MessageHandler, config.ErrorVisibilityTimeout);
                        _logger.LogError(ex, "Task failed with error: {Message}", ex.Message);
                    }
                }, stoppingToken).ContinueWith((complete) =>
                {
                    _ = semaphoreSlim.Release();
                }, stoppingToken);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Cancellation was invoked.");
        return base.StopAsync(cancellationToken);
    }
}

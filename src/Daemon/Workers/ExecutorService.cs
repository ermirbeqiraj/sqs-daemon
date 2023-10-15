using Daemon.ApplicationModels;
using Daemon.ApplicationServices;
using Infrastructure.QueueService;

namespace Daemon.Workers;

public class ExecutorService : BackgroundService
{
	private readonly IQueueService _queueService;
	private readonly IConfigurationService _configurationService;
	private readonly ILogger<ExecutorService> _logger;
	private readonly IServiceProvider _serviceProvider;

	public ExecutorService(IQueueService queueService, IConfigurationService configurationService, ILogger<ExecutorService> logger, IServiceProvider serviceProvider)
	{
		_queueService = queueService;
		_configurationService = configurationService;
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var config = await _configurationService.GetConfigurations();

		var semaphoreSlim = new SemaphoreSlim(config.ApiMaxConcurrency, config.ApiMaxConcurrency);

		while (!stoppingToken.IsCancellationRequested)
		{
			var slotsAvailable = GetAvailableSlots(semaphoreSlim.CurrentCount);

			while (slotsAvailable == 0)
			{
				await Task.Delay(1_000, stoppingToken);
				slotsAvailable = GetAvailableSlots(semaphoreSlim.CurrentCount);
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
					var consumer = _serviceProvider.GetRequiredService<ConsumerService>();
					using (consumer)
					{
						await consumer.ProcessHttpCall(config, item);
					}
				}, stoppingToken).ContinueWith((complete) =>
				{
					_ = semaphoreSlim.Release();
				}, stoppingToken);
			}
		}
	}

	public static int GetAvailableSlots(int semaphoreCurrentAvailability)
	{
		return semaphoreCurrentAvailability <= Constants.HardLimits.SQS_MAX_NUMBER_OF_MESSAGES
																	? semaphoreCurrentAvailability
																	: Constants.HardLimits.SQS_MAX_NUMBER_OF_MESSAGES;
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogWarning("Cancellation was invoked.");
		return base.StopAsync(cancellationToken);
	}
}

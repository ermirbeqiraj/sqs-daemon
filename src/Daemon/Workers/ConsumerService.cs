using Daemon.ApplicationModels;
using Infrastructure.HttpService;
using Infrastructure.QueueService;
using Infrastructure.QueueService.Dto;

namespace Daemon.Workers;

public class ConsumerService : IDisposable
{
	private Timer? _timer = null;
	private readonly IApiService _apiService;
	private readonly IQueueService _queueService;
	private readonly ILogger<ConsumerService> _logger;

	public ConsumerService(IApiService apiService, IQueueService queueService, ILogger<ConsumerService> logger)
	{
		_apiService = apiService;
		_queueService = queueService;
		_logger = logger;
	}

	public async Task ProcessHttpCall(ApiSettings config, MessageResponseDto item)
	{
		try
		{
			SetupTimer(config);

			await _apiService.Request(item.MessageBody);
			await _queueService.DeleteMessage(config.QueueUrl, item.MessageHandler);
		}
		catch (Exception ex)
		{
			await _queueService.ChangeMessageVisibility(config.QueueUrl, item.MessageHandler, config.ErrorVisibilityTimeout);
			_logger.LogError(ex, "Task failed with error: {Message}", ex.Message);
		}
	}

	private void SetupTimer(ApiSettings config)
	{
		var completeTime = config.VisibilityTimeout;
		var warningTime = 70; // 70%;
		var nextInvoke = warningTime * completeTime / 100;

		if (nextInvoke < 60) // don't bother with anything less then a min
			return;

		_timer = new Timer(TimerEvent, null, nextInvoke, Timeout.Infinite);
	}

	private void TimerEvent(object? state)
	{
		_logger.LogWarning("Message consumption is taking too long.");
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_timer != null)
			{
				_timer?.Dispose();
				_timer = null;
			}
		}
	}
}

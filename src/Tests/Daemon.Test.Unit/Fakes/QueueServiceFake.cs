using Infrastructure.QueueService;
using Infrastructure.QueueService.Dto;

namespace Daemon.Test.Unit.Fakes;
internal class QueueServiceFake : IQueueService
{
    internal readonly Dictionary<string, MessageWrapper> Queue = new();
    private static object _lock = new();

    public QueueServiceFake(int messageCount)
    {
        InitQueueSetup(messageCount);
    }

    public Task ChangeMessageVisibility(string queueUrl, string receiptHandle, int timeoutInSeconds)
    {
        return Task.CompletedTask;
    }

    public Task DeleteMessage(string queueUrl, string receiptHandle)
    {
        lock (_lock)
        {
            var item = Queue[receiptHandle];
            if (item == null)
                return Task.CompletedTask;

            if (item.Timeout < DateTime.UtcNow)
                throw new InvalidOperationException("This will be an error from aws sdk side");

            Queue.Remove(receiptHandle, out _);
            return Task.CompletedTask;
        }
    }

    public Task<IReadOnlyList<MessageResponseDto>> GetMessages(string queueUrl, int maxNumberOfMessages, int visibilityTimeout, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // needs to bring back only messages that have a timeout less then now.
            // any message returned, should have an increased timeout of now + visibilityTimeout

            var readyTime = DateTime.UtcNow.AddSeconds(visibilityTimeout);
            var messagesReady = Queue.Where(x => x.Value.Timeout < readyTime)
                                     .Take(maxNumberOfMessages)
                                     .ToList();

            foreach (var (key, value) in messagesReady)
            {
                Queue[key].Timeout = readyTime;
            }

            var msgs = (IReadOnlyList<MessageResponseDto>)messagesReady
                       .Select(x => x.Value.Message)
                       .ToList();

            return Task.FromResult(msgs);
        }
    }

    private void InitQueueSetup(int messageCount)
    {
        var timeout = DateTime.UtcNow.AddSeconds(-5);
        for (int i = 0; i < messageCount; i++)
        {
            var receiptHandle = $"message-{i}-handle-{i}";
            var messageDto = new MessageResponseDto(receiptHandle, receiptHandle, "");
            Queue[receiptHandle] = new MessageWrapper(timeout, messageDto);
        }
    }
}

internal class MessageWrapper
{
    public MessageWrapper(DateTime timeout, MessageResponseDto message)
    {
        Timeout = timeout;
        Message = message;
    }

    public DateTime Timeout { get; set; }
    public MessageResponseDto Message { get; set; }
}
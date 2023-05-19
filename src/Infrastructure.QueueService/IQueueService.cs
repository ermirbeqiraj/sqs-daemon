using Infrastructure.QueueService.Dto;


namespace Infrastructure.QueueService;

public interface IQueueService
{
    Task<IReadOnlyList<MessageResponseDto>> GetMessages(string queueUrl, int maxNumberOfMessages, int visibilityTimeout, CancellationToken cancellationToken = default);
    Task DeleteMessage(string queueUrl, string receiptHandle);
    Task ChangeMessageVisibility(string queueUrl, string receiptHandle, int timeoutInSeconds);
}

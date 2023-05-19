using Amazon.SQS;
using Amazon.SQS.Model;
using Infrastructure.QueueService.Dto;

namespace Infrastructure.QueueService;

public class QueueService : IQueueService
{
    private readonly IAmazonSQS _amazonSQS;

    public QueueService(IAmazonSQS amazonSQS)
    {
        _amazonSQS = amazonSQS;
    }

    public async Task<IReadOnlyList<MessageResponseDto>> GetMessages(string queueUrl
                                                                        , int maxNumberOfMessages
                                                                        , int visibilityTimeout
                                                                        , CancellationToken cancellationToken = default)
    {
        var receiveResponse = await _amazonSQS.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            MaxNumberOfMessages = maxNumberOfMessages,
            QueueUrl = queueUrl,
            //
            // specifying visibility timeout here overrides queue settings
            // https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-visibility-timeout.html#configuring-visibility-timeout
            // 
            VisibilityTimeout = visibilityTimeout,
            AttributeNames = new List<string> { "All" },
            WaitTimeSeconds = 20
        }, cancellationToken);

        if (receiveResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("something went south with aws...");

        return receiveResponse.Messages.Select(x => new MessageResponseDto(x.MessageId, x.ReceiptHandle, x.Body)).ToList().AsReadOnly();
    }

    public async Task DeleteMessage(string queueUrl, string receiptHandle)
    {
        await _amazonSQS.DeleteMessageAsync(queueUrl, receiptHandle);
    }

    public async Task ChangeMessageVisibility(string queueUrl, string receiptHandle, int timeoutInSeconds)
    {
        await _amazonSQS.ChangeMessageVisibilityAsync(queueUrl, receiptHandle, timeoutInSeconds);
    }
}

namespace Infrastructure.QueueService.Dto;
public record MessageResponseDto(string MessageId, string MessageHandler, string MessageBody);

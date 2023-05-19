namespace Infrastructure.HttpService.Dto;

public class ApiSettingsResponse
{
    public string QueueUrl { get; set; } = string.Empty;
    public int ApiMaxConcurrency { get; set; }
    public int VisibilityTimeout { get; set; }
    public int ErrorVisibilityTimeout { get; set; }
}

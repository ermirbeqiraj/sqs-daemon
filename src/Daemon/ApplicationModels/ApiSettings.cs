namespace Daemon.ApplicationModels;

public record ApiSettings(string QueueUrl, int ApiMaxConcurrency, int VisibilityTimeout, int ErrorVisibilityTimeout)
{
    public bool ConfigurationSetupGuard()
    {
        if (string.IsNullOrEmpty(QueueUrl))
            throw new ArgumentNullException(nameof(QueueUrl));

        if (ApiMaxConcurrency < 1 || ApiMaxConcurrency > Constants.HardLimits.MAX_CONCURRENCY)
            throw new ArgumentOutOfRangeException(nameof(ApiMaxConcurrency));

        if (VisibilityTimeout < Constants.HardLimits.MIN_VISIBILITY_TIMEOUT || VisibilityTimeout > Constants.HardLimits.MAX_VISIBILITY_TIMEOUT)
            throw new ArgumentOutOfRangeException(nameof(VisibilityTimeout));

        if (ErrorVisibilityTimeout < 0 || ErrorVisibilityTimeout > Constants.HardLimits.MAX_VISIBILITY_TIMEOUT)
            throw new ArgumentOutOfRangeException(nameof(ErrorVisibilityTimeout));

        return true;
    }
}

namespace Daemon.ApplicationModels;

public static class Constants
{
    public static class HardLimits
    {
        /// <summary>
        /// Max number of concurrent connections toward the API
        /// </summary>
        public const int MAX_CONCURRENCY = 50;

        /// <summary>
        /// Min number of seconds to keep the message un-visible / inflight
        /// </summary>
        public const int MIN_VISIBILITY_TIMEOUT = 30;

        /// <summary>
        /// Max number of seconds to keep the message un-visible / inflight
        /// </summary>
        public const int MAX_VISIBILITY_TIMEOUT = 43199;
    }
}

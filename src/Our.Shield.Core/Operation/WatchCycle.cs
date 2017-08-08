namespace Our.Shield.Core.Operation
{
    /// <summary>
    /// The type of result to return for a given Web Request Watcher
    /// </summary>
    public enum WatchCycle
    {
        /// <summary>
        /// End the current request (i.e. Request has been redirected)
        /// </summary>
        Stop,
        /// <summary>
        /// Continue with the current request
        /// </summary>
        Continue,
        /// <summary>
        /// Restart the current request (i.e. Request has been rewrite-ed to another location)
        /// </summary>
        Restart,
        /// <summary>
        /// An error has occurred in the Web Request Watcher
        /// </summary>
        Error
    }
}

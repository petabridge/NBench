namespace NBench.Sdk
{
    /// <summary>
    /// NBench settings passed into the <see cref="TestRunner"/>, usually via end-user commandline.
    /// 
    /// This class is used to memoize NBench settings and record them in the benchmark output.
    /// </summary>
    public sealed class RunnerSettings
    {
        /// <summary>
        /// Indicates if we're running in concurrent mode or not.
        /// </summary>
        public bool ConcurrentModeEnabled { get; set; }

        /// <summary>
        /// Indicates if tracing is enabled or not
        /// </summary>
        public bool TracingEnabled { get; set; }
    }
}

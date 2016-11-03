using System;

namespace NBench.Tracing
{
    /// <summary>
    /// Default no-op implementation of <see cref="IBenchmarkTrace"/>. Does nothing.
    /// </summary>
    internal sealed class NoOpBenchmarkTrace : MarshalByRefObject, IBenchmarkTrace
    {
        public static readonly NoOpBenchmarkTrace Instance = new NoOpBenchmarkTrace();

        private NoOpBenchmarkTrace() { }

        public void Debug(string message)
        {
            
        }

        public void Info(string message)
        {
            
        }

        public void Warning(string message)
        {
            
        }

        public void Error(Exception ex, string message)
        {
            
        }

        public void Error(string message)
        {
            
        }
    }
}
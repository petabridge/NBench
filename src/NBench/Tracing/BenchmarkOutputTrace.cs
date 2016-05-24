using System;
using NBench.Reporting;

namespace NBench.Tracing
{
    /// <summary>
    /// An <see cref="IBenchmarkTrace"/> implementation which creates <see cref="TraceMessage"/>s
    /// and immediately writes them to the <see cref="IBenchmarkOutput"/> implementation used for this spec.
    /// </summary>
    internal sealed class BenchmarkOutputTrace : IBenchmarkTrace
    {
        private readonly IBenchmarkOutput _benchmarkOutput;

        public BenchmarkOutputTrace(IBenchmarkOutput benchmarkOutput)
        {
            _benchmarkOutput = benchmarkOutput;
        }

        public void Debug(string message)
        {
            WriteMessage(new Debug(message));
        }

        public void Info(string message)
        {
            WriteMessage(new Info(message));
        }

        public void Warning(string message)
        {
            WriteMessage(new Warning(message));
        }

        public void Error(Exception ex, string message)
        {
            WriteMessage(new Error(ex, message));
        }

        public void Error(string message)
        {
            WriteMessage(new Error(message));
        }

        private void WriteMessage(TraceMessage message)
        {
            // all traces go into the usual log. no special treatment for errors or warnings.
            _benchmarkOutput.WriteLine(message.ToString());
        }
    }
}
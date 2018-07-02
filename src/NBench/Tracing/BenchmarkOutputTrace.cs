// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

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
            _benchmarkOutput.Error(ex, new Error(ex, message).ToString());
        }

        public void Error(string message)
        {
            _benchmarkOutput.Error(null, (new Error(message)).ToString());
        }

        private void WriteMessage(TraceMessage message)
        {
            // all traces go into the usual log. no special treatment for errors or warnings.
            _benchmarkOutput.WriteLine(message.ToString());
        }
    }
}


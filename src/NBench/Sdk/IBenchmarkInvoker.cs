// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Sdk
{
    /// <summary>
    ///     Used to invoke the benchmark methods on the user-defined
    ///     objects that have methods marked with <see cref="PerfBenchmarkAttribute" />.
    /// </summary>
    public interface IBenchmarkInvoker
    {
        string BenchmarkName { get; }
        void InvokePerfSetup(BenchmarkContext context);
        /// <summary>
        /// Used for <see cref="RunMode.Throughput"/> scenarios
        /// </summary>
        /// <param name="runCount">The number of runs for which we'll execute this benchmark</param>
        /// <param name="context">The context used for the run</param>
        void InvokePerfSetup(long runCount, BenchmarkContext context);
        void InvokeRun(BenchmarkContext context);

        void InvokePerfCleanup(BenchmarkContext context);
    }
}


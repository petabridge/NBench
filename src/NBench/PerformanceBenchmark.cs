// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench
{
    /// <summary>
    /// Specifies the way we want to run a particular performance test
    /// </summary>
    public enum TestMode
    {
        /// <summary>
        /// Requires at least one performance assertion and throws a 
        /// PASS / FAIL result into the log.
        /// </summary>
        Test,

        /// <summary>
        /// Performs no BenchmarkAssertions - just records the metrics and writes them to the log
        /// </summary>
        Measurement,
    }

    public enum RunMode
    {
        /// <summary>
        /// Run a fixed number of iterations of a benchmark.
        /// 
        /// Best for long-running benchmarks that need to measure things like memory, GC
        /// </summary>
        Iterations,

        /// <summary>
        /// Run a benchmark for a specified duration.
        /// 
        /// Best for small benchmarks designed to measure throughput - i.e. Operations per Second
        /// </summary>
        Throughput,
    }

    /// <summary>
    /// Marks a method on a class as being an NBench performance test
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PerfBenchmarkAttribute : Attribute
    {
        public const int DefaultNumberOfIterations = 10;
        public const TestMode DefaultTestType = NBench.TestMode.Measurement;
        public const RunMode DefaultRunType = NBench.RunMode.Iterations;
        public const int DefaultRuntimeMilliseconds = 0; //disabled

        public PerfBenchmarkAttribute()
        {
            TestMode = DefaultTestType;
            RunMode = DefaultRunType;
            NumberOfIterations = DefaultNumberOfIterations;
            RunTimeMilliseconds = DefaultRuntimeMilliseconds; 
        }

        /// <summary>
        /// Number of times this test will be run
        /// </summary>
        /// <remarks>Defaults to 10</remarks>
        public int NumberOfIterations { get; set; }

        /// <summary>
        /// For <see cref="NBench.RunMode.Throughput"/> tests, this determines
        /// the maximum amount of clock-time in milliseconds this benchmark will run.
        /// 
        /// For all other modes, this sets the timeout at which point the test will be failed.
        /// 
        /// Disabled by default in any tests using any mode other than <see cref="NBench.RunMode.Throughput"/>.
        /// Defaults to 1000ms in <see cref="NBench.RunMode.Throughput"/>.
        /// </summary>
        /// <remarks>Set to 0 to disable.</remarks>
        public int RunTimeMilliseconds { get; set; }

        /// <summary>
        /// The mode in which this performance data will be tested.
        /// 
        /// Defaults to <see cref="NBench.TestMode.Measurement"/>
        /// </summary>
        public TestMode TestMode { get; set; }

        /// <summary>
        /// The mode in which this performance data will be collected.
        /// 
        /// Defaults to <see cref="NBench.RunMode.Iterations"/>
        /// </summary>
        public RunMode RunMode { get; set; }

        /// <summary>
        /// A description of this performance benchmark, which will be written into the report.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If populated, this benchmark will be skipped and the skip reason will be written into the report.
        /// </summary>
        public string Skip { get; set; }

        /// <summary>
        /// Skips warmups (aside from the pre-warmup) entirely
        /// </summary>
        public bool SkipWarmups { get; set; }
    }

    /// <summary>
    /// Performs a setup operation before the <see cref="PerfBenchmarkAttribute"/> gets run.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PerfSetupAttribute : Attribute
    {
    }

    /// <summary>
    /// Performs a cleanup operation after the <see cref="PerfBenchmarkAttribute"/> gets run.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PerfCleanupAttribute : Attribute
    {
    }
}


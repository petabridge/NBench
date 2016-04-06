// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench
{
    /// <summary>
    /// Used to measure and assert how much time elapses while executing a block of code.
    /// 
    /// Typically designed for work with <see cref="RunMode.Iterations"/> benchmarks 
    /// that are longer-running (can be measured in whole milliseconds.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TimingMeasurementAttribute : MeasurementAttribute
    {

    }

    /// <summary>
    /// Takes data from a <see cref="TimingMeasurementAttribute"/> and performs an assertion
    /// on it basedon <see cref="MaxTimeMilliseconds"/> and <see cref="MinTimeMilliseconds"/> (the latter is optional.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ElapsedTimeAssertionAttribute : TimingMeasurementAttribute
    {
        /// <summary>
        /// The maximum amount of time allowed to elapse for this <see cref="PerfBenchmarkAttribute"/> in MILLISECONDS
        /// </summary>
        public long MaxTimeMilliseconds { get; set; }

        /// <summary>
        /// OPTIONAL. The minimum amount of time that must elapsed for this <see cref="PerfBenchmarkAttribute"/> in MILLISECONDs
        /// </summary>
        public long MinTimeMilliseconds { get; set; }
    }
}


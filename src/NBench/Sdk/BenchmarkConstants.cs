// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench.Sdk
{
    /// <summary>
    /// Set of constants used for evaluating different types of benchmarks and default behavior
    /// </summary>
    public static class BenchmarkConstants
    {
        /// <summary>
        /// For long-running tests, sample metrics at 10ms intervals
        /// </summary>
        public static readonly long SamplingPrecisionTicks = TimeSpan.FromMilliseconds(10).Ticks;
    }
}


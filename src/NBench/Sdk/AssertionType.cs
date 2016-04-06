// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Sdk
{
    /// <summary>
    /// Different types of BenchmarkAssertions we might use against metrics
    /// </summary>
    public enum AssertionType
    {
        /// <summary>
        /// Test against the averages of totals
        /// </summary>
        Total,

        /// <summary>
        /// Test against the totals per second
        /// </summary>
        Throughput,
    }
}


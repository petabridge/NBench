// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench
{
    /// <summary>
    /// Comparison and test types used by NBench for performing performance test BenchmarkAssertions
    /// </summary>
    public enum MustBe
    {
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        ExactlyEqualTo,
        Between,
    }
}


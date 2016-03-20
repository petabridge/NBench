// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using NBench.Metrics;

namespace NBench.Sdk
{
    /// <summary>
    /// Interface for all individual benchmark settings
    /// </summary>
    public interface IBenchmarkSetting : IEquatable<IBenchmarkSetting>
    {
        MetricName MetricName { get; }
        AssertionType AssertionType { get; }
        Assertion Assertion { get; }
    }
}


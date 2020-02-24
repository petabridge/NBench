// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Collection;
using NBench.Metrics;

namespace NBench.Tests
{
    /// <summary>
    /// Metric collector used for unit testing NBench
    /// </summary>
    public class TestMetricCollector : MetricCollector
    {
        public long CollectorValue { get; set; }

        public TestMetricCollector(MetricName name, string unitName) : base(name, unitName)
        {
        }

        public override double Collect()
        {
            return CollectorValue;
        }
    }
}


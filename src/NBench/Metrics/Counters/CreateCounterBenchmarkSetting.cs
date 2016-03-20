// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Collection;
using NBench.Collection.Counters;
using NBench.Sdk;
using NBench.Util;

namespace NBench.Metrics.Counters
{
    /// <summary>
    /// Special internal class needed to pass in a <see cref="AtomicCounter"/> to a <see cref="CounterSelector"/>.
    /// </summary>
    internal sealed class CreateCounterBenchmarkSetting : IBenchmarkSetting {
        public CreateCounterBenchmarkSetting(CounterBenchmarkSetting benchmarkSetting, AtomicCounter counter)
        {
            BenchmarkSetting = benchmarkSetting;
            Counter = counter;
        }

        public CounterBenchmarkSetting BenchmarkSetting { get; }

        public AtomicCounter Counter { get; }

        public MetricName MetricName => BenchmarkSetting.CounterName;
        public AssertionType AssertionType => BenchmarkSetting.AssertionType;
        public Assertion Assertion => BenchmarkSetting.Assertion;
        public bool Equals(IBenchmarkSetting other)
        {
            return other is CreateCounterBenchmarkSetting &&
                   ((CreateCounterBenchmarkSetting) other).BenchmarkSetting.Equals(BenchmarkSetting);
        }
    }
}


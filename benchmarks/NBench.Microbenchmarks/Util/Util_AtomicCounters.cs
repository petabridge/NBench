// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using NBench.Util;

namespace NBench.Microbenchmarks.Util
{
    [LegacyJitX64Job]
    [LegacyJitX86Job]
    [RyuJitX64Job]
    public class Util_AtomicCounters
    {
        private readonly AtomicCounter _counter = new AtomicCounter();

        [Benchmark(Description = "Count how quickly we can increment AtomicCounter")]
        public void Increment()
        {
            _counter.Increment();
        }

        [Benchmark(Description = "Count how quickly we can increment and get AtomicCounter")]
        public long GetAndIncrement()
        {
            return _counter.GetAndIncrement();
        }
    }
}


// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet;
using BenchmarkDotNet.Tasks;
using NBench.Util;

namespace NBench.Microbenchmarks.Util
{
    [BenchmarkTask(platform:BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.LegacyJit)]
    [BenchmarkTask(platform: BenchmarkPlatform.X86, jitVersion: BenchmarkJitVersion.LegacyJit)]
    [BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.RyuJit)]
    public class Util_AtomicCounters
    {
        private readonly AtomicCounter _counter = new AtomicCounter();

        [Benchmark("Count how quickly we can increment AtomicCounter")]
        public void Increment()
        {
            _counter.Increment();
        }

        [Benchmark("Count how quickly we can increment and get AtomicCounter")]
        public long GetAndIncrement()
        {
            return _counter.GetAndIncrement();
        }
    }
}


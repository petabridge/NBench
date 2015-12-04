// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Tasks;
using NBench.Sdk;

namespace NBench.Microbenchmarks.SDK
{
    [BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.LegacyJit)]
    [BenchmarkTask(platform: BenchmarkPlatform.X86, jitVersion: BenchmarkJitVersion.LegacyJit)]
    [BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.RyuJit)]
    public class Sdk_ActionBenchmarkInvoker
    {
        private readonly IBenchmarkInvoker _actionInvoker = new ActionBenchmarkInvoker("foo", context => {}, context => {}, context => {});

        [BenchmarkDotNet.Benchmark("How quickly can we invoke when injecting context in an ActionBenchmarkInvoker")]
        public void InvokeRunWithContext()
        {
            _actionInvoker.InvokeRun(BenchmarkContext.Empty);
        }
    }
}


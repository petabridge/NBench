// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using NBench.Sdk;

namespace NBench.Microbenchmarks.SDK
{
    [LegacyJitX64Job]
    [LegacyJitX86Job]
    [RyuJitX64Job]
    public class Sdk_ActionBenchmarkInvoker
    {
        private readonly IBenchmarkInvoker _actionInvoker = new ActionBenchmarkInvoker("foo", context => { },
            context => { }, context => { });

        [Benchmark(Description = "How quickly can we invoke when injecting context in an ActionBenchmarkInvoker")]
        public void InvokeRunWithContext()
        {
            _actionInvoker.InvokeRun(BenchmarkContext.Empty);
        }
    }
}
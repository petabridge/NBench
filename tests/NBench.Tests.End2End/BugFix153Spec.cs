// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Metrics.Counters;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using NBench.Tests.End2End.SampleBenchmarks;
using Xunit;

namespace NBench.Tests.End2End
{
    /// <summary>
    /// Used to verify that https://github.com/petabridge/NBench/issues/153 is fixed
    /// </summary>
    public class BugFix153Spec
    {
        [Fact]
        public void BugFix153IsFixed()
        {
            var o = new ActionBenchmarkOutput(benchmarkAction: r =>
            {
                var name = new CounterMetricName(CounterThroughputBenchmark.CounterName);
                var count = r.Data.StatsByMetric[name];
                Assert.True(count.PerSecondStats.Average > 0.0);
            });
            var d = new ReflectionDiscovery(o, DefaultBenchmarkAssertionRunner.Instance, new RunnerSettings());
            var benchmarks = d.FindBenchmarks(typeof(CounterThroughputBenchmark));
            foreach (var b in benchmarks)
            {
                b.Run();
                b.Finish();
                Assert.True(b.AllAssertsPassed);
            }
        }
    }
}

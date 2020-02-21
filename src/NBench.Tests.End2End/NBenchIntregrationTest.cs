// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using NBench.Tests.Assembly;
using Xunit;

namespace NBench.Tests.End2End
{
    public class NBenchIntregrationTest

    {
        private static readonly IBenchmarkOutput _benchmarkOutput = new ActionBenchmarkOutput(report => { }, results =>
         {
             foreach (var assertion in results.AssertionResults)
             {
                 Assert.True(assertion.Passed, results.BenchmarkName + " " + assertion.Message);
             }
         });

        private readonly IDiscovery _discovery = new ReflectionDiscovery(_benchmarkOutput);

        [Fact]
        public void ShouldPassAllBenchmarks()
        {
            if (!TestRunner.IsMono) // this spec currently hits a runtime exception with Mono: https://bugzilla.xamarin.com/show_bug.cgi?id=43291
            {
                var benchmarks = _discovery.FindBenchmarks(GetType().GetTypeInfo().Assembly).ToList();
                Assert.True(benchmarks.Count >= 1);
                Benchmark.PrepareForRun(); // force some GC here
                for (var i = 0; i < benchmarks.Count; i++)
                {
                    Benchmark.PrepareForRun(); // force some GC here
                    benchmarks[i].Run();
                    benchmarks[i].Finish();
                }
            }
        }

        [Fact]
        public void LoadAssemblyCorrect()
        {
            var package = LoadPackage();
            var result = TestRunner.Run(package);
            result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
            result.ExecutedTestsCount.Should().NotBe(0);
            result.IgnoredTestsCount.Should().Be(0);
        }

        [Fact]
        public void RunnerIncludeInvalidName()
        {
            var package = LoadPackage(new string[] { "unknown" });

            var result = TestRunner.Run(package);
            result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
            result.ExecutedTestsCount.Should().Be(0);
            result.IgnoredTestsCount.Should().NotBe(0);
        }

        [Fact]
        public void RunnerIncludePattern()
        {
            var package = LoadPackage(new[] { "*.ConfigBenchmark*" });

            var result = TestRunner.Run(package);
            result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
            result.ExecutedTestsCount.Should().Be(1);
            result.IgnoredTestsCount.Should().NotBe(0);
        }

        [Fact]
        public void RunnerIncludeMultiplePattern()
        {
            var package = LoadPackage(new[] { "*CounterThroughputBenchmark*", "*SimpleCounterBenchmark*" });

            var result = TestRunner.Run(package);
            result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
            result.ExecutedTestsCount.Should().Be(2);
            result.IgnoredTestsCount.Should().NotBe(0);
        }

        [Fact]
        public void RunnerExcludePattern()
        {
            var package = LoadPackage(null, new[] { "*CounterThroughputBenchmark*", "*SimpleCounterBenchmark*" });

            var result = TestRunner.Run(package);
            result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
            result.ExecutedTestsCount.Should().Be(1);
            result.IgnoredTestsCount.Should().NotBe(0);
        }

        private static TestPackage LoadPackage(IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            var package = NBenchRunner.CreateTest<ConfigBenchmark>();

            if (include != null)
                foreach (var i in include)
                {
                    package.AddInclude(i);
                }

            if (exclude != null)
                foreach (var e in exclude)
                {
                    package.AddExclude(e);
                }

            return package;
        }
    }
}


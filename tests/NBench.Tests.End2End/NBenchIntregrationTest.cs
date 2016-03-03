﻿// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using Xunit;

namespace NBench.Tests.End2End
{
    public class NBenchIntregrationTest
    {
        private static readonly IBenchmarkOutput _benchmarkOutput = new ActionBenchmarkOutput(report => {}, results =>
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
            var benchmarks = _discovery.FindBenchmarks(GetType().Assembly).ToList();
            Assert.True(benchmarks.Count >= 1);
            Benchmark.PrepareForRun(); // force some GC here
            for (var i = 0; i < benchmarks.Count; i++)
            {
                Benchmark.PrepareForRun(); // force some GC here
                benchmarks[i].Run();
                benchmarks[i].Finish();
            }
        }

        [Fact]
        public void LoadAssemblyCorrect()
        {
	        var package = LoadPackage();
	        var result = TestRunner.Run(package);
            Assert.True(result.AllTestsPassed);
			Assert.NotEqual(0, result.ExecutedTestsCount);
			Assert.Equal(0, result.IgnoredTestsCount);
		}

		
	    [Fact]
		public void RunnerIncludeInvalidName()
		{
			var package = LoadPackage(new string[] { "unknown" });

			var result = TestRunner.Run(package);
			Assert.True(result.AllTestsPassed);
			Assert.Equal(0, result.ExecutedTestsCount);
			Assert.NotEqual(0, result.IgnoredTestsCount);
		}

		[Fact]
		public void RunnerIncludePattern()
		{
			var package = LoadPackage(new [] { "*.ConfigBenchmark*" });

			var result = TestRunner.Run(package);
			Assert.True(result.AllTestsPassed);
			Assert.Equal(1, result.ExecutedTestsCount);
			Assert.NotEqual(0, result.IgnoredTestsCount);
		}
		[Fact]
		public void RunnerExcludePattern()
		{
			var package = LoadPackage(null, new [] { "*CounterThroughputBenchmark*", "*SimpleCounterBenchmark*" });

			var result = TestRunner.Run(package);
			Assert.True(result.AllTestsPassed);
			Assert.Equal(1, result.ExecutedTestsCount);
			Assert.NotEqual(0, result.IgnoredTestsCount);
		}

		private static TestPackage LoadPackage(IEnumerable<string> include = null, IEnumerable<string> exclude = null)
		{
#if DEBUG
	var package = new TestPackage(@"..\..\..\NBench.Tests.Assembly\bin\Debug\NBench.Tests.Assembly.dll", include, exclude);
#else
			var package = new TestPackage(@"..\..\..\NBench.Tests.Assembly\bin\Release\NBench.Tests.Assembly.dll", include, exclude);
#endif

			package.Validate();
			return package;
		}
	}
}


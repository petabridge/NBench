// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Reporting;

namespace NBench.Sdk
{
    /// <summary>
    ///     Responsible for running <see cref="Assertion" />s over <see cref="BenchmarkResults" />.
    /// </summary>
    public class DefaultBenchmarkAssertionRunner : IBenchmarkAssertionRunner
    {
        public static readonly DefaultBenchmarkAssertionRunner Instance = new DefaultBenchmarkAssertionRunner();

        private DefaultBenchmarkAssertionRunner() { }

        public IReadOnlyList<AssertionResult> RunAssertions(BenchmarkSettings settings, BenchmarkResults results)
        {
            Contract.Requires(settings != null);
            var assertionResults = new List<AssertionResult>();

            // Not in testing mode, therefore we don't need to apply these BenchmarkAssertions
            if (settings.TestMode == TestMode.Measurement)
            {
                return assertionResults;
            }

            // collect all benchmark settings with non-empty BenchmarkAssertions
            IReadOnlyList<IBenchmarkSetting> allSettings = settings.Measurements.Where(x => !x.Assertion.Equals(Assertion.Empty)).ToList();

            foreach (var setting in allSettings)
            {
                var stats = results.StatsByMetric[setting.MetricName];
                double valueToBeTested;
                if (setting.AssertionType == AssertionType.Throughput)
                {
                    valueToBeTested = stats.PerSecondStats.Average;
                }
                else
                {
                    valueToBeTested = stats.Stats.Average;
                }
                var assertionResult = AssertionResult.CreateResult(setting.MetricName, stats.Unit, valueToBeTested,
                    setting.Assertion);
                assertionResults.Add(assertionResult);
            }

            return assertionResults;
        }
    }
}
// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NBench.Reporting;

namespace NBench.Sdk
{
    /// <summary>
    /// Responsible for running <see cref="Assertion" />s over <see cref="BenchmarkResults" />.
    /// </summary>
    public interface IBenchmarkAssertionRunner
    {
        /// <summary>
        /// Based on the provided <see cref="BenchmarkSettings"/> and the <see cref="BenchmarkResults"/>
        /// collected from running a <see cref="Benchmark"/>, determine if all of the configured BenchmarkAssertions
        /// pass or not.
        /// </summary>
        /// <param name="settings">The settings for this benchmark.</param>
        /// <param name="results">The results from this benchmark.</param>
        /// <returns>A set of individual <see cref="AssertionResult"/> instances.</returns>
        IReadOnlyList<AssertionResult> RunAssertions(BenchmarkSettings settings, BenchmarkResults results);
    }
}


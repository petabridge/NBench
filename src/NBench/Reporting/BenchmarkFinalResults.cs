// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NBench.Sdk;

namespace NBench.Reporting
{
    /// <summary>
    /// <see cref="BenchmarkResults"/> with <see cref="Assertion"/> data provided.
    /// </summary>
    public class BenchmarkFinalResults
    {
        public BenchmarkFinalResults(BenchmarkResults data, IReadOnlyList<AssertionResult> assertionResults)
        {
            Data = data;
            AssertionResults = assertionResults;
        }

        public string BenchmarkName => Data.BenchmarkName;

        public BenchmarkResults Data { get; private set; }

        public IReadOnlyList<AssertionResult> AssertionResults { get; private set; }
    }
}


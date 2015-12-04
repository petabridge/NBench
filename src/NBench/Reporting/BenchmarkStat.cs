// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace NBench.Reporting
{
    /// <summary>
    /// Represents aggregate statics for a benchmark across multiple runs
    /// </summary>
    public struct BenchmarkStat
    {
        public static readonly double[] SafeValues = {0.0d};

        public BenchmarkStat(IEnumerable<long> values)
            : this(values.Select(x => (double)x).ToList())
        { }

        public BenchmarkStat(IEnumerable<double> raw)
        {
            Contract.Requires(raw != null);
            IReadOnlyList<double> values = raw.ToList();
            if (values.Count == 0) // perform a swap if the collection we receive is empty
                values = SafeValues;
            Min = values.Min();
            Max = values.Max();
            Average = values.Average();
            var n = values.Count;
            var average = Average; // need a closure for Linq methods on members of a struct
            StandardDeviation = n == 1 ? 0 : Math.Sqrt(values.Sum(x => Math.Pow(x - average, 2))/(n - 1));
            StandardError = StandardDeviation/Math.Sqrt(n);
        }

        public double Min { get; }

        public double Max { get; }

        public double Average { get; }

        public double StandardDeviation { get; }

        public double StandardError { get; }
    }
}


// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using NBench.Metrics;

namespace NBench.Collection.GarbageCollection
{
    /// <summary>
    ///     Returns the number of times <see cref="System.GC" /> has collected <see cref="Generation" />.
    /// </summary>
    public class GcCollectionsPerGenerationCollector : MetricCollector
    {
        public GcCollectionsPerGenerationCollector(MetricName name, int generation)
            : this(name, MetricNames.GcCollectionsUnits, generation)
        {
        }

        public GcCollectionsPerGenerationCollector(MetricName name, string unitName, int generation) : base(name, unitName)
        {
            Generation = generation;
        }

        /// <summary>
        ///     Refers to the number of the GC generation we're monitoring
        /// </summary>
        public int Generation { get; }

        public override double Collect()
        {
            return GC.CollectionCount(Generation);
        }
    }
}


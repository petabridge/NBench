// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench.Metrics
{
    /// <summary>
    /// Used to uniquely represent the name of a metric in a manner-agnostic way.
    /// </summary>
    public abstract class MetricName : IEquatable<MetricName>
    {
        public abstract bool Equals(MetricName other);

        /// <summary>
        /// Print the <see cref="MetricName"/> in a human-friendly manner that will be
        /// used in subsequent reports.
        /// </summary>
        /// <returns>a human-optimized string</returns>
        public abstract string ToHumanFriendlyString();
    }
}


// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using NBench.Metrics;

namespace NBench.Sdk
{
    public struct AssertionResult
    {
        public AssertionResult(MetricName metricName, string message, bool passed)
        {
            Contract.Requires(metricName != null);
            MetricName = metricName;
            Message = message;
            Passed = passed;
        }

        public MetricName MetricName { get; private set; }

        public string Message { get; private set; }
        public bool Passed { get; private set; }

        public static AssertionResult CreateResult(MetricName name, string unitName, double value, Assertion assertion)
        {
            var passed = assertion.Test(value);
            var passedString = passed ? "[PASS]" : "[FAIL]";
            var message = $"{passedString} Expected {name} to {assertion} {unitName}; actual value was {value:n} {unitName}.";
            return new AssertionResult(name, message, passed);
        }
    }
}


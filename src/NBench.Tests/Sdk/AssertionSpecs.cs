// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Sdk;
using Xunit;

namespace NBench.Tests.Sdk
{
    public class AssertionSpecs
    {
        [Theory]
        [InlineData(MustBe.ExactlyEqualTo, 1.0d, null, MustBe.ExactlyEqualTo, 1.0d, null)]
        [InlineData(MustBe.ExactlyEqualTo, 1.0d, null, MustBe.ExactlyEqualTo, 1.0d, 2.0d)]
        [InlineData(MustBe.Between, 1.0d, 1.0d, MustBe.ExactlyEqualTo, 1.0d, null)]
        [InlineData(MustBe.ExactlyEqualTo, 0.5d, null, MustBe.ExactlyEqualTo, 1.0d, null)]
        [InlineData(MustBe.ExactlyEqualTo, 1.0d, 10.0d, MustBe.ExactlyEqualTo, 1.0d, null)]
        public void Assertion_should_be_equal_by_value(MustBe condition1, double value1, double? maxValue1,
            MustBe condition2, double value2, double? maxValue2)
        {
            var expectedValue = condition1 == condition2 
                && value1.Equals(value2) 
                && maxValue1.Equals(maxValue2);

            var assertion1 = new Assertion(condition1, value1, maxValue1);
            var assertion2 = new Assertion(condition2, value2, maxValue2);

            var actualValue = assertion1.Equals(assertion2);
            Assert.Equal(expectedValue, actualValue);
        }

        [Theory]
        [InlineData(MustBe.ExactlyEqualTo, 1.0d, null, 1.0d, true)]
        [InlineData(MustBe.ExactlyEqualTo, 1.0d, null, 0.9999999d, false)]
        [InlineData(MustBe.Between, 0.99d, 1.0d, 1.000001d, false)]
        [InlineData(MustBe.Between, 0.9999999d, 1.0d, 0.9999999d, true)]
        [InlineData(MustBe.Between, -0.1d, 0.1d, 0, true)]
        [InlineData(MustBe.GreaterThan, -0.1d, null, 0, true)]
        [InlineData(MustBe.GreaterThan, -0.1d, null, -0.11, false)]
        [InlineData(MustBe.GreaterThan, -0.1d, null, -0.1, false)]
        [InlineData(MustBe.GreaterThanOrEqualTo, -0.1d, null, 0, true)]
        [InlineData(MustBe.GreaterThanOrEqualTo, -0.1d, null, -0.11, false)]
        [InlineData(MustBe.GreaterThanOrEqualTo, -0.1d, null, -0.1, true)]
        [InlineData(MustBe.LessThan, -0.1d, null, 0, false)]
        [InlineData(MustBe.LessThan, -0.1d, null, -0.11, true)]
        [InlineData(MustBe.LessThan, -0.1d, null, -0.1, false)]
        [InlineData(MustBe.LessThanOrEqualTo, -0.1d, null, 0, false)]
        [InlineData(MustBe.LessThanOrEqualTo, -0.1d, null, -0.11, true)]
        [InlineData(MustBe.LessThanOrEqualTo, -0.1d, null, -0.1, true)]
        public void Assertion_should_correctly_test_against_expected_result(MustBe condition1, double value1, double? maxValue1,
            double testValue, bool expectedValue)
        {
            var assertion1 = new Assertion(condition1, value1, maxValue1);
            var actualValue = assertion1.Test(testValue);
            Assert.Equal(expectedValue, actualValue);
        }
    }
}


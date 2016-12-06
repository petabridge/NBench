// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;

namespace NBench.Sdk
{
    /// <summary>
    /// No-op <see cref="Assertion"/>
    /// </summary>
    public sealed class EmptyAssertion : Assertion {
        public EmptyAssertion() : base(MustBe.Between, double.MinValue, double.MaxValue)
        {
        }

        public override string ToString()
        {
            return "measured only";
        }
    }

    /// <summary>
    /// Executes an assertion against a given metric
    /// </summary>
    public class Assertion
    {
        /// <summary>
        /// Empty assertion - used when in <see cref="TestMode.Measurement"/>.
        /// </summary>
        public static readonly Assertion Empty = new EmptyAssertion();

        public Assertion(MustBe condition, double value, double? maxValue)
        {
            Contract.Requires(condition != MustBe.Between || maxValue.HasValue);
            Condition = condition;
            Value = value;
            MaxValue = maxValue;
        }

        /// <summary>
        /// The condition we're using to test against <see cref="Value"/>
        /// and possibly <see cref="MaxValue"/>.
        /// </summary>
        public MustBe Condition { get; }
        
        /// <summary>
        /// The expected value we'll be testing against during <see cref="Test"/>.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Optional value used in <see cref="MustBe.Between"/> comparisons.
        /// </summary>
        public double? MaxValue { get; }

        public bool Test(double testValue)
        {
            switch (Condition)
            {
                case MustBe.Between:
                    return testValue >= Value && testValue <= MaxValue;
                case MustBe.GreaterThan:
                    return testValue > Value;
                case MustBe.GreaterThanOrEqualTo:
                    return testValue >= Value;
                case MustBe.LessThan:
                    return testValue < Value;
                case MustBe.LessThanOrEqualTo:
                    return testValue <= Value;
                case MustBe.ExactlyEqualTo:
                default:
                    return testValue.Equals(Value);
            }
        }

        private bool Equals(Assertion other)
        {
            return Condition == other.Condition && Value.Equals(other.Value) && MaxValue.Equals(other.MaxValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Assertion && Equals((Assertion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Condition;
                hashCode = (hashCode*397) ^ Value.GetHashCode();
                hashCode = (hashCode*397) ^ MaxValue.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Assertion left, Assertion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Assertion left, Assertion right)
        {
            return !Equals(left, right);
        }

        public static string MustBeToString(MustBe condition)
        {
            switch (condition)
            {
                case MustBe.Between:
                    return "must be between";
                case MustBe.ExactlyEqualTo:
                    return "must be exactly";
                case MustBe.GreaterThan:
                    return "must be greater than";
                case MustBe.GreaterThanOrEqualTo:
                    return "must be greater than or equal to";
                case MustBe.LessThan:
                    return "must be less than";
                case MustBe.LessThanOrEqualTo:
                    return "must be less than or equal to";
                default:
                    return "(unspecified)";
            }
        }

        public override string ToString()
        {
            if(Condition != MustBe.Between)
                return MustBeToString(Condition) + " " + Value.ToString("N");
            return MustBeToString(Condition) + " " + Value.ToString("N") + " and " + MaxValue?.ToString("N");
        }
    }
}


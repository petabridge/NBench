// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace NBench.Util
{
    /// <summary>
    /// Atomic counter class used for incrementing and decrementing <c>long</c> integer values.
    /// </summary>
    public class AtomicCounter
    {
        protected long Value;

        public AtomicCounter(long seed = 0)
        {
            Value = seed;
        }

        public void Increment()
        {
            Interlocked.Increment(ref Value);
        }

        public void Increment(long value)
        {
            Interlocked.Add(ref Value, value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref Value);
        }

        public void Decrement(long value)
        {
            Interlocked.Add(ref Value, -value);
        }

        public long Current => Interlocked.Read(ref Value);

        public long GetAndIncrement()
        {
            return Interlocked.Increment(ref Value) - 1;
        }
    }
}


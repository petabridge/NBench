// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench
{
    /// <summary>
    /// Handy class for being able to use pre-calculated constant values for computing byte sizes
    /// </summary>
    public static class ByteConstants
    {
        public const long EightKb = 1 << 13;
        public const long SixteenKb = 1 << 14;
        public const long ThirtyTwoKb = 1 << 15;
        public const long SixtyFourKb = 1 << 16;
    }
}


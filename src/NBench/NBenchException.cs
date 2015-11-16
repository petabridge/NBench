// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench
{
    /// <summary>
    /// Exceptions thrown by NBench
    /// </summary>
    public class NBenchException : Exception
    {
        public NBenchException(string message, Exception innerException) : base(message, innerException) { }

        public NBenchException(string message) : base(message) { }
    }
}


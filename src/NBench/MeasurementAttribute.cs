// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench
{
    /// <summary>
    /// Abstract base class used by all Measurements in NBench
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MeasurementAttribute : Attribute { }
}


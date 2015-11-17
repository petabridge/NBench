using System;

namespace NBench
{
    /// <summary>
    /// Abstract base class used by all Measurements in NBench
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MeasurementAttribute : Attribute { }
}
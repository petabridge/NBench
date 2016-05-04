// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Sys;
using System.Management;

namespace NBench.SysInfo.Windows
{
    /// <summary>
    /// Obtain Processor (CPU) information using Windows Management Instrumentation (WMI).
    /// <seealso cref="https://msdn.microsoft.com/en-us/library/aa394104(v=vs.85).aspx"/>
    /// <seealso cref="https://msdn.microsoft.com/en-us/library/aa394373(v=vs.85).aspx"/>
    /// </summary>
    /// <remarks>
    /// Explore the available properties using PowerShell:
    /// Get-WmiObject -Class Win32_ComputerSystemProcessor
    /// Get-WmiObject -Class Win32_Processor
    /// </remarks>
    public sealed class WmiProcessorSysInfo : WmiSysInfo, ISysInfo
    {
        public WmiProcessorSysInfo()
            : base("SELECT * FROM Win32_Processor")
        {
        }

        protected override string GetKeyPrefix(ManagementBaseObject processor)
        {
            return string.Format("{0}|", processor["DeviceID"]);
        }
    }
}
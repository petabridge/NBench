// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Sys;
using System.Management;

namespace NBench.SysInfo.Windows
{
    /// <summary>
    /// Obtain Physical Memory (RAM) information using Windows Management Instrumentation (WMI).
    /// </summary>
    /// <remarks>
    /// Explore the available properties using PowerShell:
    /// Get-WmiObject -Class Win32_PhysicalMemory
    /// </remarks>
    public sealed class WmiPhysicalMemorySysInfo : WmiSysInfo, ISysInfo
    {
        public WmiPhysicalMemorySysInfo()
            : base("SELECT * FROM Win32_PhysicalMemory")
        {
        }

        protected override string GetKeyPrefix(ManagementBaseObject memoryUnit)
        {
            return string.Format("RAM {0}|", memoryUnit["DeviceLocator"]);
        }
    }
}

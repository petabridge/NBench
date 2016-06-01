// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Sys;
using System.Management;

namespace NBench.SysInfo.Windows
{
    /// <summary>
    /// Obtain BIOS information using Windows Management Instrumentation (WMI).
    /// </summary>
    /// <remarks>
    /// Explore the available properties using PowerShell:
    /// Get-WmiObject -Class Win32_BIOS
    /// </remarks>
    public class WmiBiosSysInfo : WmiSysInfo, ISysInfo
    {
        public WmiBiosSysInfo()
            : base("SELECT * FROM Win32_BIOS")
        {
        }

        protected override string GetKeyPrefix(ManagementBaseObject bios)
        {
            return "BIOS|";
        }
    }
}

// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

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
    public sealed class WmiSysInfo : ISysInfo
    {
        private readonly ManagementScope _managementScope;
        private readonly SelectQuery _wmiQuery;

        public WmiSysInfo()
        {
            var wmiPath = @"\\localhost\root\cimv2";
            _managementScope = new ManagementScope(wmiPath);

            var wmiSelect = "SELECT * FROM Win32_Processor";
            _wmiQuery = new SelectQuery(wmiSelect);
        }

        public void LoadSysInfo(IDictionary<string, string> info)
        {
            if (info == null)
                return;

            using (var searcher = new ManagementObjectSearcher(_managementScope, _wmiQuery))
            {
                var processorList = searcher.Get();

                foreach (var processor in processorList)
                {
                    var keyPrefix = string.Format("{0}|", processor["DeviceID"]);

                    foreach (var processorProperty in processor.Properties)
                    {
                        if (IsReportable(processorProperty))
                        {
                            var keyName = keyPrefix + processorProperty.Name;
                            info.Add(keyName, processorProperty.Value.ToString());
                        }
                    }
                }
            }
        }

        private static bool IsReportable(PropertyData processorProperty)
        {
            bool mustInclude = true;

            mustInclude = mustInclude && !processorProperty.IsArray;

            mustInclude = mustInclude && !processorProperty.Name.StartsWith("__");

            mustInclude = mustInclude && (processorProperty.Value != null);
            mustInclude = mustInclude && !string.IsNullOrWhiteSpace(processorProperty.Value.ToString());

            return mustInclude;
        }
    }
}
// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Sys;
using System.Collections.Generic;
using System.Management;

namespace NBench.SysInfo.Windows
{
    /// <summary>
    /// Query Method for Windows Management Instrumentation (WMI).
    /// </summary>
    public abstract class WmiSysInfo : ISysInfo
    {
        private readonly ManagementScope _managementScope;
        private readonly SelectQuery _wmiQuery;

        protected WmiSysInfo(string wmiSelect)
        {
            var wmiPath = @"\\localhost\root\cimv2";
            _managementScope = new ManagementScope(wmiPath);

            _wmiQuery = new SelectQuery(wmiSelect);
        }

        protected abstract string GetKeyPrefix(ManagementBaseObject mbo);

        protected virtual string GetKeyName(string keyPrefix, PropertyData pd)
        {
            return keyPrefix + pd.Name;
        }
        protected virtual string GetValueString(PropertyData pd)
        {
            return (pd.Value == null) ? string.Empty : pd.Value.ToString();
        }
        protected virtual bool IsReportable(PropertyData processorProperty)
        {
            bool mustInclude = true;

            mustInclude = mustInclude && !processorProperty.IsArray;

            mustInclude = mustInclude && !processorProperty.Name.StartsWith("__");

            mustInclude = mustInclude && (processorProperty.Value != null);
            mustInclude = mustInclude && !string.IsNullOrWhiteSpace(processorProperty.Value.ToString());

            return mustInclude;
        }
        public void LoadSysInfo(IDictionary<string, string> info)
        {
            if (info == null)
                return;

            using (var searcher = new ManagementObjectSearcher(_managementScope, _wmiQuery))
            {
                var wmiSearchResults = searcher.Get();

                foreach (var wmiInfo in wmiSearchResults)
                {
                    var keyPrefix = GetKeyPrefix(wmiInfo);

                    foreach (var wmiProperty in wmiInfo.Properties)
                    {
                        var keyName = GetKeyName(keyPrefix, wmiProperty);

                        if (IsReportable(wmiProperty) &&
                            !info.ContainsKey(keyName))
                        {
                            info.Add(keyName, GetValueString(wmiProperty));
                        }
                    }
                }
            }
        }
    }
}

// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace NBench.SysInfo.Windows.Tests
{
    public class WmiSysInfoSpec
    {
        [Fact]
        public void LoadWmiSysInfo()
        {
            var info = new SortedDictionary<string, string>();

            var wmiSysInfo = new NBench.SysInfo.Windows.WmiSysInfo();
            wmiSysInfo.LoadSysInfo(info);

            Assert.NotEmpty(info);

            Debug.WriteLine(string.Empty.PadLeft(25, '=') + " Win32_Processor:");
            foreach (var key in info.Keys)
            {
                Debug.WriteLine(string.Format(" {0}: {1}", key, info[key]));
            }
            Debug.WriteLine(string.Empty.PadLeft(25, '=') + " Win32_Processor (END)");
        }
    }
}
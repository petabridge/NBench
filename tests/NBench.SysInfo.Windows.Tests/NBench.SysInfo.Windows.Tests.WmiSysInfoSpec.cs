// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using System.Reflection;

namespace NBench.SysInfo.Windows.Tests
{
    public class WmiSysInfoSpec
    {
        [Fact]
        public void LoadWmiProcessorSysInfo()
        {
            var hardwareInfo = new SortedDictionary<string, string>();

            var wmiSysInfo = new NBench.SysInfo.Windows.WmiProcessorSysInfo();
            wmiSysInfo.LoadSysInfo(hardwareInfo);

            Assert.NotEmpty(hardwareInfo);

            DebugWriteAll(hardwareInfo, "Win32_Processor");
        }

        [Fact]
        public void LoadWmiMemorySysInfo()
        {
            var hardwareInfo = new SortedDictionary<string, string>();

            var wmiSysInfo = new NBench.SysInfo.Windows.WmiPhysicalMemorySysInfo();
            wmiSysInfo.LoadSysInfo(hardwareInfo);

            Assert.NotEmpty(hardwareInfo);

            DebugWriteAll(hardwareInfo, "Win32_PhysicalMemory");
        }

        [Fact]
        public void LoadWmiBiosSysInfo()
        {
            var hardwareInfo = new SortedDictionary<string, string>();

            var wmiSysInfo = new NBench.SysInfo.Windows.WmiBiosSysInfo();
            wmiSysInfo.LoadSysInfo(hardwareInfo);

            Assert.NotEmpty(hardwareInfo);

            DebugWriteAll(hardwareInfo, "Win32_BIOS");
        }

        [Fact]
        public void LoadAllWmISysInfo()
        {
            var hardwareInfo = new SortedDictionary<string, string>();

            var theAssembly = Assembly.GetAssembly(typeof(WmiSysInfo));
            var wmisiTypes = from t in theAssembly.GetTypes()
                             where t.BaseType == typeof(WmiSysInfo)
                             select t;

            foreach (var t in wmisiTypes)
            {
                var wmisi = Activator.CreateInstance(t);
                ((WmiSysInfo)wmisi).LoadSysInfo(hardwareInfo);
            }

            Assert.NotEmpty(hardwareInfo);

            DebugWriteAll(hardwareInfo, "FULL HARDWARE INFO");
        }

        private static void DebugWriteAll(IDictionary<string, string> hardwareInfo, string label)
        {
            Debug.WriteLine(string.Empty.PadLeft(25, '=') + " " + label + ":");
            foreach (var key in hardwareInfo.Keys)
            {
                Debug.WriteLine(string.Format(" {0}: {1}", key, hardwareInfo[key]));
            }
            Debug.WriteLine(string.Empty.PadLeft(25, '=') + label + " (END)");
        }
    }
}
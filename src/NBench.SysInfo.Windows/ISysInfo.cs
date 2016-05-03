// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace NBench.SysInfo.Windows
{
    public interface ISysInfo
    {
        void LoadSysInfo(IDictionary<string, string> info);
    }
}

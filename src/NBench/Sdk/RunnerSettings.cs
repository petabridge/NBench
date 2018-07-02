// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Sdk
{
    /// <summary>
    /// NBench settings passed into the <see cref="TestRunner"/>, usually via end-user commandline.
    /// 
    /// This class is used to memoize NBench settings and record them in the benchmark output.
    /// </summary>
    public sealed class RunnerSettings
    {
        /// <summary>
        /// Indicates if we're running in concurrent mode or not.
        /// </summary>
        public bool ConcurrentModeEnabled { get; set; }

        /// <summary>
        /// Indicates if tracing is enabled or not
        /// </summary>
        public bool TracingEnabled { get; set; }
    }
}


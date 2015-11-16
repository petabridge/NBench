// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet;
using NBench.Microbenchmarks.SDK;
using NBench.Microbenchmarks.Util;

namespace NBench.Microbenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var competitionSwitch = new BenchmarkCompetitionSwitch(new []
            {
                typeof(Util_AtomicCounters), 
                typeof(Sdk_ActionBenchmarkInvoker),
                typeof(Sdk_ReflectionBenchmarkInvoker)
            });

            competitionSwitch.Run(args);;
        }
    }
}


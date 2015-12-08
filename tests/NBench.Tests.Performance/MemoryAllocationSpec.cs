// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace NBench.Tests.Performance
{
    public class MemoryAllocationSpec
    {
        private const int NumOfByteArrays = 1000;
        private const int ByteArraySize = 1024; // 1kb
        private const long FudgeFactor = ByteConstants.SixteenKb;
        private readonly IList<byte[]> byteArrays = new List<byte[]>(NumOfByteArrays);

        [PerfBenchmark(Description = "Simple test that allocates many byte arrays into a list",
            RunMode = RunMode.Iterations, NumberOfIterations = 13,
            TestMode = TestMode.Measurement)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo,
            NumOfByteArrays*ByteArraySize + FudgeFactor)]
        public void ByteArrayAllocation()
        {
            for (var i = 0; i < NumOfByteArrays; i++)
            {
                byteArrays.Add(new byte[ByteArraySize]);
            }
        }
    }
}


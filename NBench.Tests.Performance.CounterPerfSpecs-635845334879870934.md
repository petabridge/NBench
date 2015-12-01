# NBench.Tests.Performance.CounterPerfSpecs
_12/1/2015 2:24:47 AM_
### System Info
```ini
NBench=NBench, Version=0.0.2.0, Culture=neutral, PublicKeyToken=null
OS=Microsoft Windows NT 6.2.9200.0
ProcessorCount=8
CLR=4.0.30319.42000,IsMono=$False,MaxGcGeneration=$2
WorkerThreads=2047, IOThreads=$8
```

### NBench Settings
```ini
RunMode=Throughput, TestMode=Test
NumberOfIterations=3, MaximumRunTime=00:00:01
```

## Data
-------------------

### Totals
          Metric |           Units |             Max |         Average |             Min |          StdDev |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
TotalBytesAllocated |           bytes |            0.00 |            0.00 |            0.00 |            0.00 |
TotalCollections [Gen2] |     collections |            0.00 |            0.00 |            0.00 |            0.00 |
[Counter] TestCounter |      operations |   24,077,987.00 |   24,077,987.00 |   24,077,987.00 |            0.00 |

### Per-second Totals
          Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
TotalBytesAllocated |           bytes |            0.00 |            0.00 |            0.00 |            0.00 |
TotalCollections [Gen2] |     collections |            0.00 |            0.00 |            0.00 |            0.00 |
[Counter] TestCounter |      operations |   73,492,365.77 |   72,910,670.01 |   72,162,676.05 |      680,264.65 |

## Assertions

* [PASS] Expected [Counter] TestCounter to must be greater than 10,000,000.00 operations; actual value was 72,910,670.01 operations.
* [PASS] Expected TotalCollections [Gen2] to must be exactly 0.00 collections; actual value was 0.00 collections.
* [PASS] Expected TotalBytesAllocated to must be less than or equal to 16,384.00 bytes; actual value was 0.00 bytes.


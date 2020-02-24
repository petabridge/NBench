# NBench Quickstart Tutorial
Given that NBench users are often on the edge of performance, NBench doesn't ship with any kind of external runner as of NBench 2.0. Instead, NBench is delivered as a [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) - so it can be used in any version of .NET Framework 4.6.1 or higher and any version of .NET Core 2.0 or higher.

## Create a new Console Project
To install NBench, create a new console application and add it to your .NET solution:

![Create new Console Application and add it to your solution.](/images/install/create-project.png)

## Install `NBench` Package from NuGet
Install the most recent version of NBench into your brand new console application:

```
PS> Install-Package NBench
```

Now, we need to create at least one benchmark class that NBench will run.

## Create One or More Benchmarks
To create a benchmark, create a C# class with a default constructor:

[!code-csharp[CombinedPerfSpecs.cs](../../src/NBench.Tests.Performance/CombinedPerfSpecs.cs)]

This particular benchmark is going to measure throughput, memory, and GC.

Now that we've added at least one benchmark, NBench has work it can do. Add the following code to the `Program.cs` file in our console application project:

[!code-csharp[CombinedPerfSpecs.cs](../../src/NBench.Tests.Performance/Program.cs)]

The [`NBenchRunner` class](../api/NBench.NBenchRunner.yml) will automatically pick up the set of [NBench runner arguments](running.md) from the commandline.

## Run NBench
Execute NBench simply by running your console application in Release mode:

```
dotnet run [MyConsoleApp.dll] -c Release
```

And from there you should get some performance metrics that look like this:
-------------------

# NBench.Tests.Performance.CombinedPerfSpecs+Benchmark
__Test to gauge the impact of having multiple things to measure on a benchmark.__
_2/24/2020 7:47:27 PM_
### System Info
```ini
NBench=NBench, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null
OS=Microsoft Windows NT 6.2.9200.0
ProcessorCount=2
CLR=3.1.1,IsMono=False,MaxGcGeneration=2
```

### NBench Settings
```ini
RunMode=Throughput, TestMode=Test
NumberOfIterations=3, MaximumRunTime=00:00:01
Concurrent=True
Tracing=True
```

## Data
-------------------

### Totals
|          Metric |           Units |             Max |         Average |             Min |          StdDev |
|---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
|TotalBytesAllocated |           bytes |       16,384.00 |        8,192.00 |            0.00 |        8,192.00 |
|TotalCollections [Gen2] |     collections |            0.00 |            0.00 |            0.00 |            0.00 |
|[Counter] TestCounter |      operations |   24,175,490.00 |   24,175,490.00 |   24,175,490.00 |            0.00 |

### Per-second Totals
|          Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
|---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
|TotalBytesAllocated |           bytes |       63,670.83 |       31,803.54 |            0.00 |       31,835.46 |
|TotalCollections [Gen2] |     collections |            0.00 |            0.00 |            0.00 |            0.00 |
|[Counter] TestCounter |      operations |   93,949,794.71 |   92,576,346.36 |   90,111,628.06 |    2,139,166.52 |

### Raw Data
#### TotalBytesAllocated
|           Run # |           bytes |       bytes / s |      ns / bytes |
|---------------- |---------------- |---------------- |---------------- |
|               1 |            0.00 |            0.00 |  268,283,800.00 |
|               2 |        8,192.00 |       31,739.80 |       31,506.19 |
|               3 |       16,384.00 |       63,670.83 |       15,705.78 |

#### TotalCollections [Gen2]
|           Run # |     collections | collections / s |ns / collections |
|---------------- |---------------- |---------------- |---------------- |
|               1 |            0.00 |            0.00 |  268,283,800.00 |
|               2 |            0.00 |            0.00 |  258,098,700.00 |
|               3 |            0.00 |            0.00 |  257,323,500.00 |

#### [Counter] TestCounter
|           Run # |      operations |  operations / s | ns / operations |
|---------------- |---------------- |---------------- |---------------- |
|               1 |   24,175,490.00 |   90,111,628.06 |           11.10 |
|               2 |   24,175,490.00 |   93,667,616.30 |           10.68 |
|               3 |   24,175,490.00 |   93,949,794.71 |           10.64 |


## Benchmark Assertions

* [PASS] Expected [Counter] TestCounter to must be greater than 10,000,000.00 operations; actual value was 92,576,346.36 operations.
* [PASS] Expected TotalBytesAllocated to must be less than or equal to 32,768.00 bytes; actual value was 8,192.00 bytes.
* [PASS] Expected TotalCollections [Gen2] to must be exactly 0.00 collections; actual value was 0.00 collections.
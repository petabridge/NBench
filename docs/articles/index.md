# Introduction to NBench

[![Join the chat at https://gitter.im/petabridge/NBench](https://badges.gitter.im/petabridge/NBench.svg)](https://gitter.im/petabridge/NBench?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
Cross-platform performance benchmarking and testing framework for .NET applications.

NBench is designed for .NET developers who need to care about performance and want the ability to "unit test" their application's performance just like [XUnit](https://github.com/xunit/xunit) or [NUnit](http://nunit.org/) tests their application code.

## Why NBench?
NBench was originally developed to help support the [Akka.NET project](https://getakka.net/) to measure the performance of each code change to its critical areas. NBench is generally used for measuring the performance of entire feature areas, not micro-benchmarking the fastest way to unroll a `for` loop or whether it's more efficient to have an array of `struct`s or a `struct` of arrays.

You can see more about the origins of NBench from our talk at [.NET Fringe: Introducing NBench - Automated Performance Testing and Benchmarking for .NET](https://www.youtube.com/watch?v=EWWMAhAERFg):

<iframe width="560" height="315" src="https://www.youtube.com/embed/EWWMAhAERFg" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Frequently Asked Questions

### Who Should Use NBench?
NBench is designed to support the following types of cases:

1. Macro-level benchmarks to test the throughput, memory, and Garbage Collection (GC) performance of entire features;
2. Writing XUnit-style assertions to validate that observed performance never falls below an acceptable limit; or
3. Benchmarking the performance asynchonrous and concurrent code.

If you have any one of these cases, then NBench is for you.

### Does NBench Support .NET Core / .NET Framework Version {x}?
Given that NBench users are often on the edge of performance, NBench doesn't ship with any kind of external runner as of NBench 2.0. Instead, NBench is delivered as a [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) - so it can be used in any version of .NET Framework 4.6.1 or higher and any version of .NET Core 2.0 or higher.

Users install NBench into a stand-alone Console Application and run that application to generate their benchmarks. To learn more, see [NBench Quickstart Tutorial](quickstart.md).

### What sorts of things can NBench help me measure?
NBench is highly extensible, but out of the box we support assertions and benchmarking measurements for:

1. Throughput;
2. Memory utilization;
3. Total elapsed runtime; and
4. Garbage collection.

You can learn more by reading the [Measurements and Assertions](measurements.md) page.

### How can I get started with NBench?
Please see our [NBench Quickstart Tutorial](quickstart.md).
# Introduction to NBench

[![Join the chat at https://gitter.im/petabridge/NBench](https://badges.gitter.im/petabridge/NBench.svg)](https://gitter.im/petabridge/NBench?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
Cross-platform performance benchmarking and testing framework for .NET applications.

NBench is designed for .NET developers who need to care about performance and want the ability to "unit test" their application's performance just like [XUnit](https://github.com/xunit/xunit) or [NUnit](http://nunit.org/) tests their application code.

## Why NBench?
NBench was originally developed to help support the [Akka.NET project](https://getakka.net/) to measure the performance of each code change to its critical areas. NBench is generally used for measuring the performance of entire feature areas, not micro-benchmarking the fastest way to unroll a `for` loop or whether it's more efficient to have an array of `struct`s or a `struct` of arrays.

You can see more about the origins of NBench from our talk at [.NET Fringe: Introducing NBench - Automated Performance Testing and Benchmarking for .NET](https://www.youtube.com/watch?v=EWWMAhAERFg):

<iframe width="560" height="315" src="https://www.youtube.com/embed/EWWMAhAERFg" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>


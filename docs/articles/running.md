# Running NBench
NBench is always installed into your own console application and run from there, as we covered in the [NBench Quickstart Tutorial](quickstart.md).

Despite that, NBench has specific command line arguments it listens for and uses despite being hosted inside your own application:

dotnet run [YourNBench.dll] [--output {dir-path}] [--configuration {file-path}] [--include MyTest*.Perf*,Other*Spec] [--exclude *Long*] [--concurrent {true|false}] [--trace {true|false}] [--teamcity] [--diagnostic]

## Commandline Arguments

* **assembly names** - list of assemblies to load and test. Space delimited. Requires `.dll` or `.exe` at the end of each assembly name
* **--output-directory path** - folder where a Markdown report will be exported. Report will [look like this](https://gist.github.com/Aaronontheweb/8e0bfa2cccc63f5bd8bf)
* **--configuration path** - folder with a config file to be used when loading the `assembly names`
* **--include name test pattern** - a "`,`"(comma) separted list of wildcard pattern to be mached and included in the tests. Default value is `*` (all)
The test is executed on the complete name of the benchmark `Namespace.Class+MethodName`
* **--exclude name test pattern** - a "`,`"(comma) separted list of wildcard pattern to be mached and excluded in the tests. Default value is (none)
The test is executed on the complete name of the benchmark `Namespace.Class+MethodName`
* **-- concurrent true|false** - disables thread priority and processor affinity operations for all benchmarks. Used only when running multi-threaded benchmarks. Set to `false` (single-threaded) by default.
* **--trace true|false** - turns on trace capture inside the NBench runner and will save any captured messages to all available output targets, including Markdown reports. Set to `false` by default.
* **--diagnostic** - turns on diagnostic logging inside the `NBench.Runner` and `dotnet-nbench` executables.
* **--teamcity** - turns on TeamCity message formatting.

Supported wildcard patterns are `*` any string and `?` any char. In order to include a class with all its tests in the benchmark
you need to specify a pattern finishing in `*`. E.g. `include=*.MyBenchmarkClass.*`.

Example patterns:

```
--include "*MyBenchmarkClass*" (include all benchmarks in MyBenchmarkClass)
--include "*MyBenchmarkClass+MyBenchmark" (include MyBenchmark in MyBenchmarkClass)
--include "*MyBenchmarkClass*,*MyOtherBenchmarkClass*" (include all benchmarks in MyBenchmarkClass and MyOtherBenchmarkClass)

--exclude "*MyBenchmarkClass* "(exclude all benchmarks in MyBenchmarkClass)
--exclude "*MyBenchmarkClass+MyBenchmark" (exclude MyBenchmark in MyBenchmarkClass)
--exclude "*MyBenchmarkClass*,*MyOtherBenchmarkClass*" (exclude all benchmarks in MyBenchmarkClass and MyOtherBenchmarkClass)
```
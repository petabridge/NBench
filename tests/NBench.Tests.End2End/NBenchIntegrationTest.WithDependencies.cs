// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Akka.Tests.Performance.Actor;
using FluentAssertions;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using NBench.Tests.Assembly;
using Xunit;
using Xunit.Abstractions;

namespace NBench.Tests.End2End
{

    public class NBenchIntregrationTestWithDependenciesLoadAssembly
    {
        private readonly ITestOutputHelper _output;

        public NBenchIntregrationTestWithDependenciesLoadAssembly(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact()]
        public void LoadAssemblyCorrect()
        {
            if (!TestRunner.IsMono) // this test doesn't pass yet on Mono
            {
                var package = LoadPackageWithDependencies();
                var result = TestRunner.Run(package);
                result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
                result.ExecutedTestsCount.Should().NotBe(0);
                result.IgnoredTestsCount.Should().Be(0);
            }
        }

        private static TestPackage LoadPackageWithDependencies(IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            var package = NBenchRunner.CreateTest<ActorThroughputSpec>();

            if (include != null)
                foreach (var i in include)
                {
                    package.AddInclude(i);
                }

            if (exclude != null)
                foreach (var e in exclude)
                {
                    package.AddExclude(e);
                }

            return package;
        }
    }
}


using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace NBench.Execution.Tests
{
    public class MsBuildHelperSpecs
    {
        public static readonly string SingleTargetFramework = @"
            <Project Sdk=""Microsoft.NET.Sdk"">
              <Import Project=""..\..\src\common.props"" />
              <PropertyGroup>
                <TargetFramework>netcoreapp1.1</TargetFramework>

                <IsPackable>false</IsPackable>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""$(MicrosoftSdkVersion)"" />
                <PackageReference Include=""xunit"" Version=""$(XunitVersion)"" />
                <PackageReference Include=""xunit.runner.visualstudio"" Version=""$(XunitVersion)"" />
                <PackageReference Include=""FluentAssertions"" Version=""$(FluentAssertionsVersion)"" />
                <DotNetCliToolReference Include=""dotnet-xunit"" Version=""$(XunitVersion)"" />
              </ItemGroup>

              <ItemGroup>
                <ProjectReference Include=""..\..\src\NBench.Execution\NBench.Execution.csproj"" />
              </ItemGroup>

            </Project>";

        public static readonly string MultiTargetFramework = @"
            <Project Sdk=""Microsoft.NET.Sdk"">
              <Import Project=""..\..\src\common.props"" />
              <PropertyGroup>
                <TargetFrameworks>net452;netcoreapp1.1</TargetFrameworks>

                <IsPackable>false</IsPackable>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""$(MicrosoftSdkVersion)"" />
                <PackageReference Include=""xunit"" Version=""$(XunitVersion)"" />
                <PackageReference Include=""xunit.runner.visualstudio"" Version=""$(XunitVersion)"" />
                <PackageReference Include=""FluentAssertions"" Version=""$(FluentAssertionsVersion)"" />
                <DotNetCliToolReference Include=""dotnet-xunit"" Version=""$(XunitVersion)"" />
              </ItemGroup>

              <ItemGroup>
                <ProjectReference Include=""..\..\src\NBench.Execution\NBench.Execution.csproj"" />
              </ItemGroup>

            </Project>";

        [Fact(DisplayName = "MSBuild: Should be able to parse single target framework")]
        public void Should_parse_single_target_Framework()
        {
            var doc = new XmlDocument();
            doc.LoadXml(SingleTargetFramework);
            MsBuildHelpers.GetTargetFrameworks(doc).Should().BeEquivalentTo(new[] {"netcoreapp1.1"});
        }

        [Fact(DisplayName = "MSBuild: Should be able to parse multiple target frameworks")]
        public void Should_parse_multiple_target_Frameworks()
        {
            var doc = new XmlDocument();
            doc.LoadXml(MultiTargetFramework);
            MsBuildHelpers.GetTargetFrameworks(doc).Should().BeEquivalentTo(new[] { "net452", "netcoreapp1.1" });
        }
    }
}

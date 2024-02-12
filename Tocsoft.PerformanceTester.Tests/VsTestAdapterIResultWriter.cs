using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Discovery;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using System.Linq;
using Castle.Components.DictionaryAdapter.Xml;
using System.Text.RegularExpressions;

namespace Tocsoft.PerformanceTester.Tests
{
    public class VsTestAdapterIResultWriter : IDisposable
    {
        private readonly VsTestAdapter adapter;

        private readonly string testAssembleLocation;

        private readonly Fake<IRunContext> runContext;
        private readonly Fake<IFrameworkHandle> frameworkHandle;

        private readonly string _resultsFolder;
        private string runSettingsXml = null;
        public void Dispose()
        {
            Directory.Delete(_resultsFolder, true);
        }

        public VsTestAdapterIResultWriter()
        {
            _resultsFolder = Path.GetFullPath(Path.Combine(".", "TempTestResults", Guid.NewGuid().ToString()));
            Directory.CreateDirectory(_resultsFolder);
            this.runContext = new Fake<IRunContext>();
            var runSettings = new Fake<IRunSettings>();
            runContext.CallsTo(x => x.RunSettings).Returns(runSettings.FakedObject);
            runSettings.CallsTo(x => x.SettingsXml).ReturnsLazily(() => runSettingsXml);

            runContext.CallsTo(x => x.TestRunDirectory)
                .Returns(_resultsFolder);

            this.adapter = new VsTestAdapter();
            this.testAssembleLocation = Path.GetFullPath(new Uri(typeof(Benchmarking.Benchmarks.UnitTest1).Assembly.Location).LocalPath);
            this.frameworkHandle = new Fake<IFrameworkHandle>();

        }

        public TestResult RunTest(string name)
        {
            List<TestCase> testCases = new List<TestCase>();
            List<TestResult> results = new();

            var logger = new Fake<IMessageLogger>();
            var sink = new Fake<ITestCaseDiscoverySink>();

            frameworkHandle.CallsTo(x => x.RecordResult(A<TestResult>.Ignored))
                .Invokes((TestResult c) =>
                {
                    results.Add(c);
                });

            sink.CallsTo(x => x.SendTestCase(A<TestCase>.Ignored))
                .Invokes((TestCase c) =>
                {
                    testCases.Add(c);
                });

            adapter.DiscoverTests(new[] { testAssembleLocation }, runContext.FakedObject, logger.FakedObject, sink.FakedObject);
            var testCase = Assert.Single(testCases.Where(x => x.FullyQualifiedName == name));

            adapter.RunTests(
               new[] { testCase },
               runContext.FakedObject,
               frameworkHandle.FakedObject);

            return Assert.Single(results);
        }

        private string GetCsvResults()
        {
            var csvPath = Directory.EnumerateFiles(_resultsFolder).Single();
            var csv = File.ReadAllText(csvPath);

            csv = Regex.Replace(csv, ".{8}-.{4}-.{4}-.{4}-.{12}", "<GUID>");
            csv = Regex.Replace(csv, "0\\.\\d{1,10}", "<duration>");
            return csv;
        }

        [Fact]
        public void RunTest_Full()
        {
            var results = RunTest("Benchmarking.Benchmarks.UnitTest1.Test1");

            var csv = GetCsvResults();
            Assert.Equal(TestOutcome.Passed, results.Outcome);
            Assert.Equal(@"""Test Name"",""Iteration"",""Is Warmup"",""Duration"",""Iteration Status"",""Run Status"",""Tags""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""1"",""True"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""2"",""True"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""3"",""True"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""1"",""False"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""2"",""False"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""3"",""False"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""4"",""False"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
""Benchmarking.Benchmarks.UnitTest1.Test1"",""5"",""False"",""<duration>"",""Passed"",""Passed"",""runid:<GUID>;version:1""
", csv, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void RunTest_Skipped()
        {
            var results = RunTest("Benchmarking.Benchmarks.UnitTest1.TestSkipped");

            var csv = GetCsvResults();
            Assert.Equal(TestOutcome.Skipped, results.Outcome);
            Assert.Equal(@"""Test Name"",""Iteration"",""Is Warmup"",""Duration"",""Iteration Status"",""Run Status"",""Tags""
""Benchmarking.Benchmarks.UnitTest1.TestSkipped"",,""False"",""0"",""Skipped"",""Skipped"",
", csv, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void CustomColumns()
        {
            runSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RunSettings>
<TestResults>
    <Column name=""Environment"" template=""Static Env name"" />
    <Column name=""TestName"" template=""{{TestCase.Name}}"" />
    <Column name=""Tags"" template=""{{#each Result.Tags}}{{this}}{{#unless @last}};{{/unless}}{{/each}}"" />
    <Column name=""RunId"" template=""{{#each Result.Tags}}{{#hasPrefix this 'runid:'}}{{trimPrefix this 'runid:'}}{{/hasPrefix}}{{/each}}"" />
</TestResults>
<TestRunParameters>
      <Parameter name =""paramaters"" value=""https://hublsoft.local"" />
      <Parameter name =""host"" value=""https://hublsoft.local"" />
      <Parameter name =""username"" value=""admin@anon"" />
      <Parameter name =""password"" value=""Password1!"" />
  </TestRunParameters>
</RunSettings>";
            var results = RunTest("Benchmarking.Benchmarks.UnitTest1.Test1");

            var csv = GetCsvResults();

            Assert.Equal(TestOutcome.Passed, results.Outcome);
            Assert.Equal(@"""Environment"",""TestName"",""Tags"",""RunId""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
""Static Env name"",""Benchmarking.Benchmarks.UnitTest1.Test1"",""runid:<GUID>;version:1"",""<GUID>""
", csv, ignoreLineEndingDifferences: true);
        }
    }
}

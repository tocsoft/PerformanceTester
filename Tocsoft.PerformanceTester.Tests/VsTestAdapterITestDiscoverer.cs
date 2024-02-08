using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Tocsoft.PerformanceTester.Tests
{
    public class VsTestAdapterITestDiscoverer
    {
        private readonly VsTestAdapter adapter;

        private readonly string testAssembleLocation;

        private readonly Fake<IDiscoveryContext> discoveryContext;

        private readonly Fake<IMessageLogger> logger;
        private readonly Fake<ITestCaseDiscoverySink> sink;
        private readonly List<TestCase> testCases = new List<TestCase>();
        private TestCase testCase => Assert.Single(testCases);

        public VsTestAdapterITestDiscoverer()
        {
            this.adapter = new VsTestAdapter();
            this.testAssembleLocation = Path.GetFullPath(new Uri(typeof(Benchmarking.Benchmarks.UnitTest1).Assembly.Location).LocalPath);
            this.discoveryContext = new Fake<IDiscoveryContext>();
            this.logger = new Fake<IMessageLogger>();
            this.sink = new Fake<ITestCaseDiscoverySink>();

            sink.CallsTo(x => x.SendTestCase(A<TestCase>.Ignored))
                .Invokes((TestCase c) =>
                   {
                       testCases.Add(c);
                   });
        }

        [Fact]
        public void SingleCaseDiscovered()
        {
            adapter.DiscoverTests(new[] { testAssembleLocation }, discoveryContext.FakedObject, logger.FakedObject, sink.FakedObject);

            Assert.Single(testCases);
        }

        [Fact]
        public void FullyQualifiedName()
        {
            adapter.DiscoverTests(new[] { testAssembleLocation }, discoveryContext.FakedObject, logger.FakedObject, sink.FakedObject);

            Assert.Equal("Benchmarking.Benchmarks.UnitTest1.Test1", testCase.FullyQualifiedName);
        }

        [Fact]
        public void DisplayName()
        {
            adapter.DiscoverTests(new[] { testAssembleLocation }, discoveryContext.FakedObject, logger.FakedObject, sink.FakedObject);

            Assert.Equal("Benchmarking.Benchmarks.UnitTest1.Test1", testCase.DisplayName);
        }

        [Fact]
        public void ConsistentIdGenerated()
        {
            adapter.DiscoverTests(new[] { testAssembleLocation }, discoveryContext.FakedObject, logger.FakedObject, sink.FakedObject);

            // id is generated as a NamedGuid based on testcase paramaters, id will change if we change any of the inputs however shouldn't change between runs
            Assert.Equal("92f7b5b7-575b-596c-ba65-2cbb91dcc5d3", testCase.Id.ToString());
        }
    }
}

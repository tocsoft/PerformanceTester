using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Moq;
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

        private readonly Mock<IDiscoveryContext> discoveryContext;

        private readonly Mock<IMessageLogger> logger;
        private readonly Mock<ITestCaseDiscoverySink> sink;
        private readonly List<TestCase> testCases = new List<TestCase>();
        private TestCase testCase => Assert.Single(testCases);

        public VsTestAdapterITestDiscoverer()
        {
            this.adapter = new VsTestAdapter();
            this.testAssembleLocation = Path.GetFullPath(new Uri(typeof(Benchmarking.Benchmarks.UnitTest1).Assembly.Location).LocalPath);
            this.discoveryContext = new Mock<IDiscoveryContext>();
            this.logger = new Mock<IMessageLogger>();
            this.sink = new Mock<ITestCaseDiscoverySink>();

            sink.Setup(x => x.SendTestCase(It.IsAny<TestCase>()))
           .Callback<TestCase>(c =>
           {
               testCases.Add(c);
           });
        }

        [Fact]
        public void SingleCaseDiscovered ()
        {
            adapter.DiscoverTests(new[] { testAssembleLocation }, discoveryContext.Object, logger.Object, sink.Object);
         
            Assert.Single(testCases);
        }

        [Fact]
        public void FullyQualifiedName()
        {
            adapter.DiscoverTests(new[] { testAssembleLocation }, discoveryContext.Object, logger.Object, sink.Object);

            Assert.Equal("Benchmarking.Benchmarks.UnitTest1.Test1", testCase.FullyQualifiedName);
        }

        [Fact]
        public void ConsistentIdGenerated()
        {
            adapter.DiscoverTests(new[] { testAssembleLocation }, discoveryContext.Object, logger.Object, sink.Object);

            // id is generated as a NamedGuid based on testcase paramaters, id will change if we change any of the inputs however shouldn't change between runs
            Assert.Equal("92f7b5b7-575b-596c-ba65-2cbb91dcc5d3", testCase.Id.ToString());
        }
    }
}

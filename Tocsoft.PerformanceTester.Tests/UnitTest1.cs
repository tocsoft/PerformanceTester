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
        [Fact]
        public void DiscoverTests()
        {
            var adapter = new VsTestAdapter();
            var assembleLocation = new Uri(typeof(Benchmarking.Benchmarks.UnitTest1).Assembly.Location).LocalPath;
            var discoveryContext = new Mock<IDiscoveryContext>();
            var logger = new Mock<IMessageLogger>();
            var sink = new Mock<ITestCaseDiscoverySink>();

            List<TestCase> testCases = new List<TestCase>();
            sink.Setup(x => x.SendTestCase(It.IsAny<TestCase>()))
                .Callback<TestCase>(c =>
                {
                    testCases.Add(c);
                });

            adapter.DiscoverTests(new[] {
                Path.GetFullPath(assembleLocation)
            },
            discoveryContext.Object,
            logger.Object,
            sink.Object
            );
            var tc = Assert.Single(testCases);

            Assert.Equal("Benchmarking.Benchmarks.UnitTest1.Test1", tc.FullyQualifiedName);
        }
    }
}

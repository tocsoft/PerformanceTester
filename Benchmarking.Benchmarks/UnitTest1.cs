using Microsoft.VisualStudio.TestPlatform.Common;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;
using Tocsoft.PerformanceTester;

namespace Benchmarking.Benchmarks
{
    public class UnitTest1
    {
        static int counter = 0;
        private readonly TestContext context;
        private string env;

        public UnitTest1()
        {
            this.context = TestContext.CurrentContext;
            this.env = this.context.Properties["environment"];
            context.WriteLine($"Constructor { this.env}- {Thread.CurrentThread.ManagedThreadId}");
        }

        [PerformanceBenchmark(WarmUpCount = 10, ExecutionLength = 3000, ConcurrancyCount = 10)]
        public void Test1()
        {
            Thread.Sleep(250);
        }
    }
}

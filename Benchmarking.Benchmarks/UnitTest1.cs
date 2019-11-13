using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Tocsoft.PerformanceTester;
using System.Threading.Tasks;

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
            context.WriteLine($"Constructor { this.env} {context.IsWarmup}- {Thread.CurrentThread.ManagedThreadId}");
        }

        [PerformanceBenchmark(WarmUpCount = 3, ExecutionLength = 3000, ConcurrancyCount = 10)]
        public async Task Test1()
        {
            var runid = Guid.NewGuid();
            context.WriteLine($"before async {runid}");
            await Task.Delay(250);
            context.WriteLine($"after async {runid}");
        }
    }

    public class beforeHook
    {
        [BeforeAllTests]
        public void BeforeAll()
        {
            TestContext.CurrentContext.WriteLine("Before all tests here ran once");
        }
    }
}

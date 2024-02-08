using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Tocsoft.PerformanceTester;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Benchmarking.Benchmarks
{
    public class UnitTest1
    {
        private string env;

        public UnitTest1()
        {
            var context = TestContext.CurrentContext;
            this.env = context.Parameters["environment"];
            context.WriteLine($"Constructor {this.env} {context.IsWarmup}- {Thread.CurrentThread.ManagedThreadId}");
        }

        [PerformanceBenchmark(WarmUpCount = 3, ExecutionLength = 3000, ConcurrancyCount = 1)]
        //[PerformanceBenchmark()]
        public async Task Test1()
        {
            var runid = Guid.NewGuid();
            TestContext.Tags.Add($"runid:{runid}");
            TestContext.WriteLine($"before async {runid}");
            await Task.Delay(250);
            TestContext.WriteLine($"after async {runid}");
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

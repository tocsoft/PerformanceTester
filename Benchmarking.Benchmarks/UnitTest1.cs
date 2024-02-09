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
            this.env = context.Properties["environment"];
            context.WriteLine($"Constructor {this.env} {context.IsWarmup}- {Thread.CurrentThread.ManagedThreadId}");

            context.RegisterCallback(LifecycleEvent.BeforeIteration, (ctx) =>
            {
                ctx.WriteLine($"before iteration: warm:{ctx.IsWarmup} {string.Join(" ", ctx.Tags)}");
            });
            context.RegisterCallback(LifecycleEvent.AfterIteration, (ctx) =>
            {
                ctx.WriteLine($"after iteration: warm:{ctx.IsWarmup} {string.Join(" ", ctx.Tags)}");
            });
        }

        [PerformanceBenchmark(WarmUpCount = 3, ExecutionCount = 5, ConcurrancyCount = 1)]
        //[PerformanceBenchmark()]
        public async Task Test1()
        {
            var runid = Guid.NewGuid();
            TestContext.Tags.Add($"runid:{runid}");
            TestContext.Tags.Add($"version:1");
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

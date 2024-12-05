using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public class PerformanceBenchmarkTestCase : PerformanceTestCase
    {
        private readonly MethodInfo methodInfo;
        private readonly PerformanceBenchmarkAttribute settings;

        public PerformanceBenchmarkTestCase(MethodInfo methodInfo, PerformanceBenchmarkAttribute settings)
        {
            this.methodInfo = methodInfo;
            this.settings = settings;
        }

        public override bool Skipped => methodInfo.GetCustomAttributes<SkippedAttribute>().Any();
        public override string Name => $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
        public override string DisplayName => $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
        public override string UniqueName => $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}()";
        public override IEnumerable<ITestLifecycle> BuildLifeTimeEvents()
        {
            var methods = methodInfo.ReflectedType
                            .GetRuntimeMethods();
            foreach (var m in methods)
            {
                if (m == this.methodInfo)
                {
                    continue;
                }

                var factories = m.GetCustomAttributes()
                    .OfType<ITestLifecycleFactory>();

                foreach (var f in factories)
                {
                    var eventhandlers = f.Build(m);
                    foreach (var h in eventhandlers)
                    {
                        yield return h;
                    }
                }
            }
        }

        public override async Task<IEnumerable<PerformanceTestIterationResult>> ExecuteAsync(ITestContext context)
        {
            IEnumerable<ITestLifecycle> testLifecycles = BuildLifeTimeEvents();

            var afterTest = testLifecycles.OfType<ITestLifecycleAfterTest>().ToArray();
            var beforeTest = testLifecycles.OfType<ITestLifecycleBeforeTest>().ToArray();
            var afterTestIteration = testLifecycles.OfType<ITestLifecycleAfterTestIteration>().ToArray();
            var beforeTestIteration = testLifecycles.OfType<ITestLifecycleBeforeTestIteration>().ToArray();

            var results = new ConcurrentBag<PerformanceTestIterationResult>();

            using (var executor = new MethodExecuter(this.methodInfo))
            {
                async Task RunIteration(bool warmup)
                {
                    using (var subContext = TestContext.Start(this, context, warmup))
                    {
                        foreach (var t in beforeTestIteration) { await t.BeforeTestIteration(subContext); }
                        await subContext.RunCallbacks(LifecycleEvent.BeforeIteration);
                        var result = await executor.ExecuteAsync(subContext);
                        var finalResult = new PerformanceTestIterationResult
                        {
                            Duration = result.Elapsed,
                            Error = result.Error,
                            Outcome = result.Error == null ? TestOutcome.Passed : TestOutcome.Failed,
                            IsWarmup = warmup,
                            Output = subContext.Output,
                            Tags = subContext.Tags.ToArray(),
                            MetaData = new Dictionary<string, string>(subContext.MetaData, StringComparer.OrdinalIgnoreCase),
                        };
                        results.Add(finalResult);

                        subContext.Items[typeof(PerformanceTestIterationResult)] = finalResult;

                        foreach (var t in afterTestIteration) { await t.AfterTestIteration(subContext); }
                        await subContext.RunCallbacks(LifecycleEvent.AfterIteration);
                    }
                }

                foreach (var t in beforeTest) { await t.BeforeTest(context); }
                await context.RunCallbacks(LifecycleEvent.BeforeTest);

                var wc = Math.Max(0, this.settings.WarmUpCount);
                for (var i = 0; i < wc; i++)
                {
                    await RunIteration(warmup: true);
                }
                var timeout = Stopwatch.StartNew();
                var tasks = new List<Task>();

                var cc = Math.Max(1, this.settings.ConcurrancyCount);
                for (var i = 0; i < cc; i++)
                {
                    var t = Task.Run(async () =>
                    {
                        do
                        {
                            await RunIteration(warmup: false);
                        } while (timeout.ElapsedMilliseconds < settings.ExecutionLength || results.Count < (settings.ExecutionCount + settings.WarmUpCount));
                    });

                    tasks.Add(t);
                }
                await Task.WhenAll(tasks);
                // all executions complete deal with stats etc out side the runners

                foreach (var t in afterTest) { await t.AfterTest(context); }
                await context.RunCallbacks(LifecycleEvent.AfterTest);

                return results;
            }
        }
    }

    public class MethodExecuterResult
    {
        public MethodExecuterResult(TimeSpan elapsed, Exception error)
        {
            Elapsed = elapsed;
            Error = error;
        }

        public TimeSpan Elapsed { get; }
        public Exception Error { get; }
    }

    public class MethodExecuter : IDisposable
    {
        static object[] emptyArgs = new object[0];

        private readonly Type targetType;

        private readonly ObjectMethodExecutor executor;
        private object instance;
        private readonly ParameterInfo[] paramaters;
        private bool injectContext = false;

        public MethodExecuter(MethodInfo methodInfo)
        {
            this.targetType = methodInfo.ReflectedType;
            this.executor = ObjectMethodExecutor.Create(methodInfo, targetType.GetTypeInfo());
            this.instance = targetType.GetInstance();
            this.paramaters = methodInfo.GetParameters();
        }

        public void Dispose()
        {
            try
            {
                if (instance is IDisposable d)
                {
                    d.Dispose();
                    instance = null;
                }
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        public async Task<MethodExecuterResult> ExecuteAsync(ITestContext testContext, bool throwException = false)
        {
            Exception error = null;

            var sw = Stopwatch.StartNew();
            try
            {
                var args = emptyArgs;
                if (paramaters.Length > 0)
                {
                    args = new object[this.paramaters.Length];
                    for (int i = 0; i < this.paramaters.Length; i++)
                    {
                        var target = this.paramaters[i].ParameterType;
                        if (target == typeof(ITestContext))
                        {
                            args[i] = testContext;
                        }
                        else
                        {
                            if (testContext.Items.TryGetValue(target, out var val) && target.IsAssignableFrom(val.GetType()))
                            {
                                args[i] = val;
                            }
                        }
                    }
                }

                if (executor.IsMethodAsync)
                {
                    await executor.ExecuteAsync(instance, args);
                }
                else
                {
                    executor.Execute(instance, args);
                }

                sw.Stop();
            }
            catch (Exception exception)
            {
                error = exception;
                sw.Stop();
            }

            if (throwException && error != null)
            {
                throw error;
            }

            return new MethodExecuterResult(sw.Elapsed, error);
        }
    }
}

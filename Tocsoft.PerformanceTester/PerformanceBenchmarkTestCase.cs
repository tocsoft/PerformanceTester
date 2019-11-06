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

        static object[] emptyArgs = new object[0];

        public override async Task<IEnumerable<PerformanceTestIterationResult>> ExecuteAsync(TestContext context)
        {
            var targetType = this.methodInfo.ReflectedType;
            var ex = ObjectMethodExecutor.Create(this.methodInfo, targetType.GetTypeInfo());

            //Task<IEnumerable<PerformanceTestIterationResult>> ExecuteInnerAsync(ObjectMethodExecutor executor, TypeInfo type)
            //{
            //    List<PerformanceTestIterationResult> results = new List<PerformanceTestIterationResult>();
            //}

            async Task<PerformanceTestIterationResult> ExecuteSingleAsync(ObjectMethodExecutor executor, Type type)
            {
                var result = new PerformanceTestIterationResult();

                var instance = type.GetInstance();

                var sw = Stopwatch.StartNew();
                try
                {

                    if (executor.IsMethodAsync)
                    {
                        await executor.ExecuteAsync(instance, emptyArgs);
                    }
                    else
                    {
                        executor.Execute(instance, emptyArgs);
                    }

                    sw.Stop();
                }
                catch (Exception exception)
                {
                    result.Error = exception;
                    sw.Stop();
                }

                result.Duration = sw.Elapsed;

                return result;
            }

            var results = new ConcurrentBag<PerformanceTestIterationResult>();
            for (var i = 0; i < this.settings.WarmUpCount; i++)
            {
                using (var subContext = TestContext.Start(context, true))
                {
                    var result = await ExecuteSingleAsync(ex, targetType);
                    result.IsWarmup = true;
                    result.Output = subContext.Output;
                    results.Add(result);
                }
            }
            var timeout = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < this.settings.ConcurrancyCount; i++)
            {
                var t = Task.Run(async () =>
                {
                    do
                    {
                        using (var subContext = TestContext.Start(context, false))
                        {
                            var result = await ExecuteSingleAsync(ex, targetType);
                            result.IsWarmup = false;
                            result.Output = subContext.Output;
                            results.Add(result);
                        }
                    } while (timeout.ElapsedMilliseconds < settings.ExecutionLength);
                });

                tasks.Add(t);
            }
            await Task.WhenAll(tasks);
            // all executions complete deal with stats etc out side the runners

            return results;

            // in here we are async all the way down
            // we handle newing up class/disposing of class multiple times etc here

            // new class
            // execute method
            // scoped injection??
            // time everything
            // export report
        }
    }
}

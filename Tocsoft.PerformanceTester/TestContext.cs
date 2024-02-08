using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public static class TestContext
    {
        private static AsyncLocal<ITestContext> context = new AsyncLocal<ITestContext>();

        public static ITestContext CurrentContext => context.Value;

        public static IReadOnlyDictionary<string, string> Parameters => CurrentContext?.Parameters;

        public static IDictionary<object, object> Items => CurrentContext?.Items;

        public static IList<string> Tags => CurrentContext?.Tags;

        public static PerformanceTestCase TestCase => CurrentContext?.TestCase;

        public static bool? IsWarmup => CurrentContext?.IsWarmup;

        public static void WriteLine(string str)
            => CurrentContext?.WriteLine(str);


        public static ITestContext Start(PerformanceTestCase testCase, ITestContext parentContext, bool? isWarmup)
        {
            var context = new DefaultTestContext(testCase, parentContext, isWarmup);
            TestContext.context.Value = context;
            return context;
        }

        public static ITestContext Start(PerformanceTestCase testCase, AdapterSettings settings)
        {
            var context = new DefaultTestContext(testCase, settings);
            TestContext.context.Value = context;
            return context;
        }

        public static ITestContext Start(IMessageLogger messageLogger, AdapterSettings settings)
        {
            var context = new DefaultTestContext(messageLogger, settings);
            TestContext.context.Value = context;
            return context;
        }

        internal static void RevertContext(ITestContext testContext, ITestContext parentContext)
        {
            if (context.Value == testContext)
            {
                context.Value = parentContext;
            }
        }

        public static void RegisterCallback(this ITestContext testContext, LifecycleEvent trigger, Action<ITestContext> callback)
            => testContext.RegisterCallback(trigger, c =>
            {
                callback(c);
                return Task.CompletedTask;
            });

        private class CallbackKey
        {
            public LifecycleEvent Trigger { get; set; }
        }

        public static void RegisterCallback(this ITestContext testContext, LifecycleEvent trigger, Func<ITestContext, Task> callback)
        {
            testContext.Items.Add(new CallbackKey() { Trigger = trigger }, callback);
        }

        public static async Task RunCallbacks(this ITestContext testContext, LifecycleEvent trigger)
        {
            foreach (var kvp in testContext.Items)
            {
                if (kvp.Key is CallbackKey key && kvp.Value is Func<ITestContext, Task> func && key.Trigger == trigger)
                {
                    await func(testContext);
                }
            }
        }
    }

    public enum LifecycleEvent
    {
        BeforeAll,
        AfterAll,
        BeforeTest,
        AfterTest,
        BeforeIteration,
        AfterIteration,
    }

}

using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

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
    }

    public interface ITestContext : IDisposable
    {
        IReadOnlyDictionary<string, string> Parameters { get; }

        IDictionary<object, object> Items { get; }

        IList<string> Tags { get; }

        string Output { get; }

        PerformanceTestCase TestCase { get; }

        bool? IsWarmup { get; }

        void WriteLine(string str);
    }

    internal class DefaultTestContext : ITestContext, IDisposable
    {
        static ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        internal DefaultTestContext(PerformanceTestCase testCase, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();

            this.Parameters = settings.TestProperties;
            IsWarmup = false;
            TestCase = testCase;
        }

        internal DefaultTestContext(IMessageLogger messageLogger, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();
            this.Parameters = settings.TestProperties;
            IsWarmup = false;
            this.messageLogger = messageLogger;
        }

        internal DefaultTestContext(PerformanceTestCase testCase, ITestContext parentContext, bool? isWarmup)
        {
            this.sb = stringBuilderPool.Get();
            this.Parameters = parentContext.Parameters;
            TestCase = testCase;
            this.parentContext = parentContext;
            IsWarmup = isWarmup;
        }

        private StringBuilder sb;
        private readonly ITestContext parentContext;
        private readonly IMessageLogger messageLogger;

        public bool? IsWarmup { get; }

        public IReadOnlyDictionary<string, string> Parameters { get; }

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public IList<string> Tags { get; } = new List<string>();

        public string Output => sb.ToString();

        public PerformanceTestCase TestCase { get; }

        public void Dispose()
        {
            stringBuilderPool.Return(sb);
            sb = null;
            TestContext.RevertContext(this, parentContext);
        }

        public void WriteLine(string str)
        {
            sb.AppendLine(str);
            parentContext?.WriteLine(str);
            messageLogger?.SendMessage(TestMessageLevel.Informational, str);
        }
    }
}

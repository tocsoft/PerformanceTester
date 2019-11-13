using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

namespace Tocsoft.PerformanceTester
{
    public class TestContext : IDisposable
    {
        private static AsyncLocal<TestContext> context = new AsyncLocal<TestContext>();
        public static TestContext CurrentContext => context.Value;
        public static IReadOnlyDictionary<string,string> Parameters => CurrentContext?.Properties;

        static ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
        private TestContext(PerformanceTestCase testCase, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();

            this.Properties = settings.TestProperties;
            IsWarmup = false;
            TestCase = testCase;
        }
        private TestContext(IMessageLogger messageLogger, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();
            this.Properties = settings.TestProperties;
            IsWarmup = false;
            this.messageLogger = messageLogger;
        }

        private TestContext(PerformanceTestCase testCase, TestContext parentContext, bool isWarmup)
        {
            this.sb = stringBuilderPool.Get();
            this.Properties = parentContext.Properties;
            TestCase = testCase;
            this.parentContext = parentContext;
            IsWarmup = isWarmup;
        }

        public static TestContext Start(PerformanceTestCase testCase, TestContext parentContext, bool isWarmup)
        {
            var context = new TestContext(testCase, parentContext, isWarmup);
            TestContext.context.Value = context;
            return context;
        }

        public static TestContext Start(PerformanceTestCase testCase, AdapterSettings settings)
        {
            var context = new TestContext(testCase, settings);
            TestContext.context.Value = context;
            return context;
        }
        public static TestContext Start(IMessageLogger messageLogger, AdapterSettings settings)
        {
            var context = new TestContext(messageLogger, settings);
            TestContext.context.Value = context;
            return context;
        }

        public bool IsWarmup { get; }

        private StringBuilder sb;
        private readonly TestContext parentContext;
        private readonly IMessageLogger messageLogger;

        public IReadOnlyDictionary<string, string> Properties { get; }
        public string Output => sb.ToString();

        public PerformanceTestCase TestCase { get; }

        public void Dispose()
        {
            stringBuilderPool.Return(sb);
            sb = null;
            context.Value = this.parentContext;
        }

        public void WriteLine(string str)
        {
            sb.AppendLine(str);
            parentContext?.WriteLine(str);
            messageLogger?.SendMessage(TestMessageLevel.Informational, str);
        }
    }
}

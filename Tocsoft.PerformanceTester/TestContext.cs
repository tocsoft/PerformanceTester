using Microsoft.Extensions.ObjectPool;
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

        static ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
        private TestContext(AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();

            this.Properties = settings.TestProperties;
            IsWarmup = false;
        }

        private TestContext(TestContext parentContext, bool isWarmup)
        {
            this.sb = stringBuilderPool.Get();
            this.Properties = parentContext.Properties;
            this.parentContext = parentContext;
            IsWarmup = isWarmup;
        }

        public static TestContext Start(TestContext parentContext, bool isWarmup)
        {
            var context = new TestContext(parentContext, isWarmup);
            TestContext.context.Value = context;
            return context;
        }

        public static TestContext Start(AdapterSettings settings)
        {
            var context = new TestContext(settings);
            TestContext.context.Value = context;
            return context;
        }

        public bool IsWarmup { get; }

        private StringBuilder sb;
        private readonly TestContext parentContext;

        public IReadOnlyDictionary<string, string> Properties { get; }
        public string Output => sb.ToString();

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
        }
    }
}

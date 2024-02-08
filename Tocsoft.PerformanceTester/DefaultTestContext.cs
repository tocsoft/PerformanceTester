using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tocsoft.PerformanceTester
{
    internal class DefaultTestContext : ITestContext, IDisposable
    {
        static ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        internal DefaultTestContext(PerformanceTestCase testCase, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();

            this.Parameters = settings.TestProperties;
            IsWarmup = false;
            TestCase = testCase;
            Items = new Dictionary<object, object>();
            Tags = new List<string>();
        }

        internal DefaultTestContext(IMessageLogger messageLogger, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();
            this.Parameters = settings.TestProperties;
            IsWarmup = false;
            this.messageLogger = messageLogger;
            Items = new Dictionary<object, object>();
            Tags = new List<string>();
        }

        internal DefaultTestContext(PerformanceTestCase testCase, ITestContext parentContext, bool? isWarmup)
        {
            this.sb = stringBuilderPool.Get();
            this.Parameters = parentContext.Parameters;
            TestCase = testCase;
            this.parentContext = parentContext;
            IsWarmup = isWarmup;

            Items = new Dictionary<object, object>(parentContext.Items);
            Tags = new List<string>(parentContext.Tags);
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

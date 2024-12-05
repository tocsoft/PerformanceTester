using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace Tocsoft.PerformanceTester
{
    internal class DefaultTestContext : ITestContext, IDisposable
    {
        static ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        internal DefaultTestContext(PerformanceTestCase testCase, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();

            this.Settings = settings;
            IsWarmup = false;
            TestCase = testCase;
            Items = new Dictionary<object, object>();
            Tags = new List<string>();
            MetaData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        internal DefaultTestContext(IMessageLogger messageLogger, AdapterSettings settings)
        {
            this.sb = stringBuilderPool.Get();
            IsWarmup = false;
            this.messageLogger = messageLogger;
            Items = new Dictionary<object, object>();
            Tags = new List<string>();
            MetaData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Settings = settings;
        }

        internal DefaultTestContext(PerformanceTestCase testCase, ITestContext parentContext, bool? isWarmup)
        {
            this.sb = stringBuilderPool.Get();
            TestCase = testCase;
            this.parentContext = parentContext;
            IsWarmup = isWarmup;
            Items = new Dictionary<object, object>(parentContext.Items);
            Tags = new List<string>(parentContext.Tags);
            MetaData = new Dictionary<string, string>(parentContext.MetaData, StringComparer.OrdinalIgnoreCase);
            this.Settings = parentContext.Settings;
        }

        private StringBuilder sb;
        private readonly ITestContext parentContext;
        private readonly IMessageLogger messageLogger;

        public bool? IsWarmup { get; }
        
        public AdapterSettings Settings { get; }

        public IReadOnlyDictionary<string, string> Properties => Settings.TestProperties;

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public IList<string> Tags { get; } = new List<string>();

        public IDictionary<string, string> MetaData { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

using System;
using System.Collections.Generic;

namespace Tocsoft.PerformanceTester
{
    public interface ITestContext : IDisposable
    {
        IReadOnlyDictionary<string, string> Properties { get; }

        IDictionary<object, object> Items { get; }

        IList<string> Tags { get; }

        IDictionary<string, string> MetaData { get; }

        string Output { get; }

        PerformanceTestCase TestCase { get; }

        bool? IsWarmup { get; }

        AdapterSettings Settings { get; }

        void WriteLine(string str);
    }
}

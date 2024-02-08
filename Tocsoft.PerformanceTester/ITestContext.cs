using System;
using System.Collections.Generic;

namespace Tocsoft.PerformanceTester
{
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
}

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tocsoft.PerformanceTester
{
    public class PerformanceTestIterationResult
    {
        public bool IsWarmup { get; set; }

        public TimeSpan Duration { get; set; }

        public Exception Error { get; set; }

        public string Output { get; set; }

        public TestOutcome Outcome { get; set; }

        public string[] Tags { get; set; } = Array.Empty<string>();

        public IReadOnlyDictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}

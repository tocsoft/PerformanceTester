using System;
using System.Collections.Generic;

namespace Tocsoft.PerformanceTester
{
    public class PerformanceTestIterationResult
    {
        public bool IsWarmup { get; set; }

        public TimeSpan Duration { get; set; }

        public Exception Error { get; set; }

        public string Output { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}

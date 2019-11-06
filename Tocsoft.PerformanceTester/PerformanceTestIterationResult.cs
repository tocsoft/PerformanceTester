using System;

namespace Tocsoft.PerformanceTester
{
    public class PerformanceTestIterationResult
    {
        public bool IsWarmup { get; set; }

        public TimeSpan Duration { get; set; }

        public Exception Error { get; set; }

        public string Output { get; set; }
    }
}

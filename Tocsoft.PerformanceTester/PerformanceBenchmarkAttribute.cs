using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tocsoft.PerformanceTester
{
    public class PerformanceBenchmarkAttribute : Attribute, IPerformanceTestCaseFactory
    {

        /// <summary>
        /// Get or set the count of threads to be used to execute this case.
        /// </summary>
        public long ConcurrancyCount { get; set; } = 1;


        /// <summary>
        /// Get or set the number for executions to perform before starting to measure.
        /// </summary>
        public long WarmUpCount { get; set; } = 0;

        /// <summary>
        /// Get or set the length of time to attempt to reexecute this case if a previouse run finshes. Method will execute the warm up number for times prior to starting the timer.
        /// </summary>
        /// <remarks>Leave as zero for single timed exection.</remarks>
        public long ExecutionLength { get; set; } = 0;

        /// <summary>
        /// Get or set the minimum number of times the test should execute in addition to the warm up tests.
        /// </summary>
        /// <remarks>Leave as zero for timed exection only.</remarks>
        public long ExecutionCount { get; set; } = 0;

        public IEnumerable<PerformanceTestCase> Build(MethodInfo methodInfo)
        {
            yield return new PerformanceBenchmarkTestCase(methodInfo, this);
        }
    }
}

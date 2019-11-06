using System.Collections.Generic;
using System.Reflection;

namespace Tocsoft.PerformanceTester
{
    public interface IPerformanceTestCaseFactory
    {
        IEnumerable<PerformanceTestCase> Build(MethodInfo methodInfo);
    }
}

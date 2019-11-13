using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public interface IPerformanceTestCaseFactory
    {
        IEnumerable<PerformanceTestCase> Build(MethodInfo methodInfo);
    }
}

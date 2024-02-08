using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public interface IGlobalTestLifecycleFactory
    {
        IEnumerable<ITestLifecycle> Build(MethodInfo methodInfo);
    }

    public interface ITestLifecycleFactory
    {
        IEnumerable<ITestLifecycle> Build(MethodInfo methodInfo);
    }

    public interface ITestLifecycle
    {
    }

    public interface ITestLifecycleBeforeTest : ITestLifecycle
    {
        Task BeforeTest(ITestContext testContext);
    }

    public interface ITestLifecycleBeforeTestIteration : ITestLifecycle
    {
        Task BeforeTestIteration(ITestContext iterationContext);
    }

    public interface ITestLifecycleAfterTestIteration : ITestLifecycle
    {
        Task AfterTestIteration(ITestContext iterationContext);
    }

    public interface ITestLifecycleAfterTest : ITestLifecycle
    {
        Task AfterTest(ITestContext testContext);
    }

    public interface ITestLifecycleAfterAllTests : ITestLifecycle
    {
        Task AfterAllTests(ITestContext testContext);
    }

    public interface ITestLifecycleBeforeAllTests : ITestLifecycle
    {
        Task BeforeAllTests(ITestContext testContext);
    }
}

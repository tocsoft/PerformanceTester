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
        Task BeforeTest(TestContext testContext);
    }

    public interface ITestLifecycleBeforeTestIteration : ITestLifecycle
    {
        Task BeforeTestIteration(TestContext iterationContext);
    }

    public interface ITestLifecycleAfterTestIteration : ITestLifecycle
    {
        Task AfterTestIteration(TestContext iterationContext);
    }

    public interface ITestLifecycleAfterTest : ITestLifecycle
    {
        Task AfterTest(TestContext testContext);
    }

    public interface ITestLifecycleAfterAllTests : ITestLifecycle
    {
        Task AfterAllTests(TestContext testContext);
    }

    public interface ITestLifecycleBeforeAllTests : ITestLifecycle
    {
        Task BeforeAllTests(TestContext testContext);
    }
}

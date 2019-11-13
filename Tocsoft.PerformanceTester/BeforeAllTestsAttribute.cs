using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public class BeforeAllTestsAttribute : Attribute, IGlobalTestLifecycleFactory
    {
        IEnumerable<ITestLifecycle> IGlobalTestLifecycleFactory.Build(MethodInfo methodInfo)
        {
            yield return new BeforeAllTestsMethodCall(methodInfo);
        }
    }

    public class BeforeAllTestsMethodCall : ITestLifecycleBeforeAllTests
    {
        private readonly MethodInfo methodInfo;

        public BeforeAllTestsMethodCall(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public Task BeforeAllTests(TestContext testContext)
        {
            var executor = new MethodExecuter(methodInfo);
            return executor.ExecuteAsync(true);
        }
    }
}

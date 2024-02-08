using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public class BeforeTestIterationAttribute : Attribute, IGlobalTestLifecycleFactory
    {
        IEnumerable<ITestLifecycle> IGlobalTestLifecycleFactory.Build(MethodInfo methodInfo)
        {
            yield return new BeforeAllTestsMethodCall(methodInfo);
        }
    }

    internal class BeforeTestIterationAttributeMethodCall : ITestLifecycleBeforeTestIteration
    {
        private readonly MethodInfo methodInfo;

        public BeforeTestIterationAttributeMethodCall(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public Task BeforeTestIteration(ITestContext iterationContext)
        {
            using (var executor = new MethodExecuter(methodInfo))
            {
                return executor.ExecuteAsync(true);
            }
        }
    }
}

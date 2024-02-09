using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public class BeforeTestAttribute : Attribute, ITestLifecycleFactory
    {
        IEnumerable<ITestLifecycle> ITestLifecycleFactory.Build(MethodInfo methodInfo)
        {
            yield return new BeforeTestAttributeMethodCall(methodInfo);
        }
    }

    internal class BeforeTestAttributeMethodCall : ITestLifecycleBeforeTest
    {
        private readonly MethodInfo methodInfo;

        public BeforeTestAttributeMethodCall(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public Task BeforeTest(ITestContext iterationContext)
        {
            using (var executor = new MethodExecuter(methodInfo))
            {
                return executor.ExecuteAsync(true);
            }
        }
    }
}

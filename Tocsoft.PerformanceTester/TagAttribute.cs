using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public class TagAttribute : Attribute, ITestLifecycleFactory, ITestLifecycleBeforeTest
    {
        public string[] Tags { get; set; }
        public TagAttribute(params string[] tags)
        {
            Tags = tags;
        }

        IEnumerable<ITestLifecycle> ITestLifecycleFactory.Build(MethodInfo methodInfo)
        {
            yield return this;
        }

        Task ITestLifecycleBeforeTest.BeforeTest(ITestContext iterationContext)
        {
            foreach (var t in Tags)
            {
                if (!iterationContext.Tags.Contains(t))
                {
                    iterationContext.Tags.Add(t);
                }
            }

            return Task.CompletedTask;
        }
    }
}

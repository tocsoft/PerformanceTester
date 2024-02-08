using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{
    public abstract class PerformanceTestCase
    {
        public virtual bool Skipped { get; } = false;

        public abstract string Name { get; }
        public virtual string DisplayName => Name;
        public virtual string UniqueName => Name;

        internal Guid Id => GuidNamed.Create(GuidNamed.UrlNamespace, UniqueName);

        public virtual IEnumerable<ITestLifecycle> BuildLifeTimeEvents()
        {
            return Enumerable.Empty<ITestLifecycle>();
        }

        public abstract Task<IEnumerable<PerformanceTestIterationResult>> ExecuteAsync(ITestContext context);
    }
}

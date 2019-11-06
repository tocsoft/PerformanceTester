using System;
using System.Collections.Generic;
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

        public abstract Task<IEnumerable<PerformanceTestIterationResult>> ExecuteAsync(TestContext context);
    }
}

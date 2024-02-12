using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tocsoft.PerformanceTester
{
    internal static class CsvHelper
    {
        public static void WriteCsvLine(this TextWriter writer, AdapterSettings settings, TestRunResultContext context)
            => writer.WriteCsvLine(settings.TestResultsTemplate.Select(x => (object)x.Value.Invoke(context)));

        public static void WriteCsvLine(this TextWriter writer, params object[] paramaters)
            => writer.WriteCsvLine((IEnumerable<object>)paramaters);

        public static void WriteCsvLine(this TextWriter writer, IEnumerable<object> paramaters)
        {
            var args = (paramaters ?? Enumerable.Empty<object>())
                            .Select(x => x?.ToString())
                            .Select(x => x.Replace("\"", "\"\""))
                            .Select(x => x.Length > 0 ? $"\"{x}\"" : x);

            writer.WriteLine(string.Join(",", args));
        }
    }
}

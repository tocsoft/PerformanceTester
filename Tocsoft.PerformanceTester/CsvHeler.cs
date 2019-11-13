using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tocsoft.PerformanceTester
{
    internal static class CsvHelper
    {
        public static void WriteCsvLine(this TextWriter writer, params object[] paramaters)
        {
            var args = (paramaters ?? new object[0])
                            .Select(x => x?.ToString())
                            .Select(x => x.Replace("\"", "\"\""))
                            .Select(x => x.Length > 0 ? $"\"{x}\"" : x);

            writer.WriteLine(string.Join(",", args));
        }
    }
}

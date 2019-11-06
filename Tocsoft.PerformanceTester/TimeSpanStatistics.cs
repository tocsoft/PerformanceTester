using System;
using System.Collections.Generic;
using System.Linq;

namespace Tocsoft.PerformanceTester
{
    public class TimeSpanStatistics
    {

        public static TimeSpan Correlation(IEnumerable<TimeSpan> xl, IEnumerable<TimeSpan> yl)
        {
            return TimeSpan.FromMilliseconds(Statistics.Correlation(xl.Select(x=>x.TotalMilliseconds), yl.Select(x => x.TotalMilliseconds)));
        }

        public static TimeSpan Covariance(IEnumerable<TimeSpan> xl, IEnumerable<TimeSpan> yl)
        {
            return TimeSpan.FromMilliseconds(Statistics.Covariance(xl.Select(x=>x.TotalMilliseconds), yl.Select(x=>x.TotalMilliseconds)));
        }

        public static TimeSpan Kurtosis(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.Kurtosis(list.Select(x=>x.TotalMilliseconds)));
        }

        public static TimeSpan KurtosisPop(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.KurtosisPop(list.Select(x=>x.TotalMilliseconds)));
        }

        public static TimeSpan Mean(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.Mean(list.Select(x=>x.TotalMilliseconds)));
        }


        public static TimeSpan Max(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.Max(list.Select(x=>x.TotalMilliseconds)));
        }

        public static TimeSpan Min(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.Min(list.Select(x=>x.TotalMilliseconds)));
        }


        public static TimeSpan Median(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.Median(list.Select(x=>x.TotalMilliseconds)));
        }


        public static TimeSpan Moment(IEnumerable<TimeSpan> list, int m)
        {
            return TimeSpan.FromMilliseconds(Statistics.Moment(list.Select(x=>x.TotalMilliseconds), m));
        }

        public static TimeSpan Skewness(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.Skewness(list.Select(x=>x.TotalMilliseconds)));
        }

        public static TimeSpan Variance(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Statistics.Variance(list.Select(x=>x.TotalMilliseconds)));
        }

        public static TimeSpan StandardDeviation(IEnumerable<TimeSpan> list)
        {
            return TimeSpan.FromMilliseconds(Math.Sqrt(Statistics.Variance(list.Select(x=>x.TotalMilliseconds))));
        }

        public static TimeSpan StudenttStatisticIndependent(IEnumerable<TimeSpan> xl, IEnumerable<TimeSpan> yl)
        {
            return TimeSpan.FromMilliseconds(Statistics.StudenttStatisticIndependent(xl.Select(x=>x.TotalMilliseconds), yl.Select(x=>x.TotalMilliseconds)));
        }


        public static TimeSpan WelchtStatistic(IEnumerable<TimeSpan> xl, IEnumerable<TimeSpan> yl, out double df)
        {
            return TimeSpan.FromMilliseconds(Statistics.WelchtStatistic(xl.Select(x=>x.TotalMilliseconds), yl.Select(x=>x.TotalMilliseconds), out df));
        }
    }
}

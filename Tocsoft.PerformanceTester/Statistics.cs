using System;
using System.Collections.Generic;
using System.Linq;

namespace Tocsoft.PerformanceTester
{
    public class Statistics
    {

        public static double Correlation(double[] x, double[] y)
        {
            double sx = StandardDeviation(x);
            double sy = StandardDeviation(y);
            double sum = 0.0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
                sum += x[i] * y[i];

            return (sum - n * Mean(x) * Mean(y)) / ((n - 1) * sx * sy);
        }

        public static double Correlation(IEnumerable<double> xl, IEnumerable<double> yl)
        {
            return Correlation(xl.ToArray(), yl.ToArray());
        }

        public static double Covariance(double[] x, double[] y)
        {
            double sumx = 0, sumy = 0, sumxy = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
            {
                sumx += x[i];
                sumy += y[i];
                sumxy += x[i] * y[i];
            }

            return (sumxy - sumx * sumy / n) / (n - 1);
        }

        public static double Covariance(IEnumerable<double> xl, IEnumerable<double> yl)
        {
            return Covariance(xl.ToArray(), yl.ToArray());
        }

        public static double FStatistic(double[] x, double[] y)
        {
            return Variance(x) / Variance(y);
        }

        public static double Kurtosis(double[] x)
        {
            double m2 = Moment(x, 2);
            double m4 = Moment(x, 4);

            return m4 / (m2 * m2) - 3.0;
        }

        public static double Kurtosis(IEnumerable<double> list)
        {
            return Kurtosis(list.ToArray());
        }

        public static double KurtosisPop(double[] x)
        {
            int n = x.Length;

            if (n >= 3)
            {
                return (n - 1) * ((n + 1) * Kurtosis(x) + 6) / ((n - 2) * (n - 3));
            }

            return 0;
        }

        public static double KurtosisPop(IEnumerable<double> list)
        {
            return KurtosisPop(list.ToArray());
        }

        public static double Mean(double[] x)
        {
            double sum = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
                sum += x[i];

            return sum / n;
        }

        public static double Mean(IEnumerable<double> list)
        {
            return Mean(list.ToArray());
        }

        public static double Max(double[] x)
        {
            double max = double.MinValue;

            for (int i = 0; i < x.Length; i++)
                if (x[i] > max)
                    max = x[i];

            return max;
        }

        public static double Max(IEnumerable<double> list)
        {
            return Max(list.ToArray());
        }

        public static double Min(double[] x)
        {
            double min = double.MaxValue;

            for (int i = 0; i < x.Length; i++)
                if (x[i] < min)
                    min = x[i];

            return min;
        }

        public static double Min(IEnumerable<double> list)
        {
            return Min(list.ToArray());
        }

        public static double Median(double[] x)
        {
            int n = x.Length, n2 = n / 2 - 1;

            if (n % 2 == 1)
                return x[(n + 1) / 2 - 1];

            return 0.5 * (x[n2] + x[n2 + 1]);
        }

        public static double Median(IEnumerable<double> list)
        {
            return Median(list.ToArray());
        }

        public static double Moment(double[] x, int m)
        {
            double mean = Mean(x), sum = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
                sum += Math.Pow(x[i] - mean, m);

            return sum / n;
        }

        public static double Moment(IEnumerable<double> list, int m)
        {
            return Moment(list.ToArray(), m);
        }

        public static double Skewness(double[] x)
        {
            // NIST definition of adjusted Fisher-Pearson 
            // coefficient of skewness 
            double m3 = Moment(x, 3);
            double sx = StandardDeviation(x);
            int n = x.Length, n1 = n - 1;

            return (Math.Sqrt(n * n1) / n1) * m3 / Math.Pow(sx, 3);
        }

        public static double Skewness(IEnumerable<double> list)
        {
            return Skewness(list.ToArray());
        }

        public static double Variance(double[] x)
        {
            double mean = Mean(x), sumSq = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
            {
                double delta = x[i] - mean;

                sumSq += delta * delta;
            }

            return sumSq / (n - 1);
        }

        public static double Variance(IEnumerable<double> list)
        {
            return Variance(list.ToArray());
        }

        public static double StandardDeviation(double[] x)
        {
            return Math.Sqrt(Variance(x));
        }

        public static double StandardDeviation(IEnumerable<double> list)
        {
            return Math.Sqrt(Variance(list.ToArray()));
        }

        public static double StudenttStatisticIndependent(double[] x, double[] y)
        {
            // equal variances 
            double xmean = Mean(x), ymean = Mean(y);
            double xvar = Variance(x), yvar = Variance(y);
            int xn = x.Length, yn = y.Length;
            double sxy = Math.Sqrt(((xn - 1) * xvar + (yn - 1) * yvar) / (xn + yn - 2));
            double denom = Math.Sqrt(xvar / xn + yvar / yn);

            return (xmean - ymean) / denom;
        }

        public static double StudenttStatisticIndependent(IEnumerable<double> xl, IEnumerable<double> yl)
        {
            return StudenttStatisticIndependent(xl.ToArray(), yl.ToArray());
        }

        public static double WelchtStatistic(double[] x, double[] y, out double df)
        {
            // unequal variances 
            double xmean = Mean(x), ymean = Mean(y);
            double xvar = Variance(x), yvar = Variance(y);
            int xn = x.Length, yn = y.Length;
            double xvn = xvar / xn, yvn = yvar / yn;
            double sxy = Math.Sqrt(xvn + yvn);

            df = Math.Pow(xvn + yvn, 2) / (xvn * xvn / (xn - 1) + yvn * yvn / (yn - 1));
            return (xmean - ymean) / sxy;
        }

        public static double WelchtStatistic(IEnumerable<double> xl, IEnumerable<double> yl, out double df)
        {
            return WelchtStatistic(xl.ToArray(), yl.ToArray(), out df);
        }
    }
}

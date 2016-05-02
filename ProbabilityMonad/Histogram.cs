using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    public static class Histogram
    {
        private const int DEFAULT_SCALE = 40;

        internal class Bucket {
            public double Min { get; }
            public double Max { get; }
            public Bucket(double min, double max)
            {
                Min = min;
                Max = max;
                Values = new List<double>();
                WeightedValues = new List<ItemProb<double>>();
            }

            public List<double> Values { get; set; }
            public List<ItemProb<double>> WeightedValues { get; set; }
            public string Name { get; set; }
            public int BarSize { get; set; }
        }

        internal struct BucketData
        {
            public double Min;
            public double Max;
            public double Width;
            public int NumBuckets;

            internal BucketData(double min, double max, double width, int numBuckets)
            {
                Min = min;
                Max = max;
                Width = width;
                NumBuckets = numBuckets;
            }
        }

        /// <summary>
        /// Make n evenly spaced buckets in a range
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="numBuckets"></param>
        /// <returns></returns>
        internal static List<Bucket> MakeBucketList(double min, double max, int numBuckets)
        {
            if (max == min) return new List<Bucket> { new Bucket(min, max) };
            var bucketSize = (max - min) / numBuckets;
            var bucketList = new List<Bucket>();
            var currentMin = min;
            while (currentMin < max)
            {
                bucketList.Add(new Bucket(currentMin, currentMin + bucketSize));
                currentMin += bucketSize;
            }
            return bucketList;
        }

        /// <summary>
        /// Make n evenly spaced buckets in a range
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="numBuckets"></param>
        /// <returns></returns>
        internal static List<Bucket> MakeIntBucketList(int min, int max, int numBuckets)
        {
            if (max == min) return new List<Bucket> { new Bucket(min, max) };
            var bucketSize = (max - min) / numBuckets;
            var bucketList = new List<Bucket>();
            var currentMin = min;
            while (currentMin < max + 1)
            {
                bucketList.Add(new Bucket(currentMin, currentMin + bucketSize));
                currentMin += bucketSize;
            }
            return bucketList;
        }

        /// <summary>
        /// Print a bucket
        /// </summary>
        /// <param name="buckets"></param>
        /// <returns></returns>
        internal static string ShowBuckets(IEnumerable<Bucket> buckets, double scale)
        {
            var sb = new StringBuilder();
            Func<double, string> formatDouble = d => $"{d:N2}";

            var minPadding = LongestString(buckets, b => formatDouble(b.Min));
            var maxPadding = LongestString(buckets, b => formatDouble(b.Max));
            var barScale = BarScale(buckets.Select(b => b.BarSize), scale);
            foreach (var bucket in buckets)
            {
                var min = formatDouble(bucket.Min).PadLeft(minPadding, ' ');
                var max = formatDouble(bucket.Max).PadLeft(maxPadding, ' ');
                sb.AppendLine($"{min} {max} {Bar((int) (bucket.BarSize * barScale))}");
            }
            sb.AppendLine("");
            return sb.ToString();
        }

        internal static int LongestString<A>(IEnumerable<A> list, Func<A, string> toString)
        {
            return list.Select(x => toString(x).Length).Max();
        }

        internal static double BarScale(IEnumerable<int> barLengths, double scale)
        {
            var longestBar = barLengths.Max();
            return longestBar > scale ? scale / longestBar : 1;
        }

        /// <summary>
        /// Draw bar of width n
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static string Bar(int n)
        {
            var barBuilder = new StringBuilder();
            for (var i = 0; i < n; i++)
            {
                barBuilder.Append("#");
            }
            return barBuilder.ToString();
        }

        /// <summary>
        /// Draw bar of width n
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static string Bar(int n)
        {
            var barBuilder = new StringBuilder();
            for (var i = 0; i < n; i++)
            {
                barBuilder.Append("#");
            }
            return barBuilder.ToString();
        }

        /// <summary>
        /// Return sum of list with a given value function
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="getVal"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static double Sum<A>(Func<A, double> getVal, IEnumerable<A> list)
        {
            return list.Select(getVal).Sum();
        }

        public static string Weighted(IEnumerable<ItemProb<double>> nums, int numBuckets = 10, double scale = DEFAULT_SCALE)
        {
            if (!nums.Any()) return "No data to graph.";

            var sorted = nums.OrderBy(x => x.Item);
            var min = sorted.First().Item;
            var max = sorted.Last().Item;
            var bucketList = MakeBucketList(min, max, numBuckets);

            var totalMass = Sum(ip => ip.Prob.Value, sorted.Where(ip => !Double.IsInfinity(ip.Prob.LogValue)));
            foreach (var bucket in bucketList)
            {
                bucket.WeightedValues.AddRange(sorted.Where(x => x.Item >= bucket.Min && x.Item < bucket.Max));
                bucket.BarSize = (int) Math.Floor(bucket.WeightedValues.Select(x => x.Prob.Value).Sum() / totalMass * scale);
            }
            return ShowBuckets(bucketList, scale);
        }

        public static string Unweighted(IEnumerable<double> nums, int numBuckets = 10, double scale = DEFAULT_SCALE)
        {
            if (!nums.Any()) return "No data to graph.";
            var sorted = nums.OrderBy(x => x);
            var min = sorted.First();
            var max = sorted.Last();
            var total = Sum(x => x, sorted);

            var bucketList = MakeBucketList(min, max, numBuckets);

            foreach (var bucket in bucketList)
            {
                bucket.Values.AddRange(sorted.Where(x => x >= bucket.Min && x < bucket.Max));
                bucket.BarSize = (int) Math.Floor((double)bucket.Values.Count() / sorted.Count() * scale);
            }
            return ShowBuckets(bucketList, scale);
        }

        public static string Finite<A>(Samples<A> itemProbs, Func<A, string> showFunc = null, double scale = DEFAULT_SCALE)
        {
            if (!itemProbs.Weights.Any()) return "No data to graph.";
            if (showFunc == null) showFunc = a => a.ToString();

            var normalized = Importance.Normalize(CompactUnordered(itemProbs, showFunc));
            var sb = new StringBuilder();
            var display = normalized.Weights.Select(ip => new Tuple<string, int, Prob>(showFunc(ip.Item), (int) Math.Floor(ip.Prob.Value * scale), ip.Prob));
            var maxWidth = display.Select(d => d.Item1.Length).Max();
            var barScale = BarScale(display.Select(d => d.Item2), scale);
            foreach (var line in display)
            {
                var item = line.Item1;
                var barLength = (int) (line.Item2 * barScale);
                sb.AppendLine($"{item.PadLeft(maxWidth)} {line.Item3,6} {Bar(barLength)}");
            }
            return sb.ToString();
        }
    }
}

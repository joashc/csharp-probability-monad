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
            public int BarSize { get; set; }
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
            while (currentMin < max)
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
        internal static string ShowBuckets(IEnumerable<Bucket> buckets)
        {
            var sb = new StringBuilder();
            foreach (var bucket in buckets)
            {
                var min = String.Format("{0:N2}", bucket.Min);
                var max = String.Format("{0:N2}", bucket.Max);
                sb.AppendLine($"{min,-6} {max,6}\t{Bar(bucket.BarSize)}");
            }
            sb.AppendLine("");
            return sb.ToString();
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



        public static string Weighted(IEnumerable<ItemProb<double>> nums, int numBuckets = 10, int scale = 40)
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
            return ShowBuckets(bucketList);
        }

        public static string Unweighted(IEnumerable<double> nums, int numBuckets = 10, int scale = 40)
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
            return ShowBuckets(bucketList);
        }

        public static string Finite<A>(Samples<A> itemProbs, Func<A, string> showFunc = null, double scale = 100)
        {
            if (!itemProbs.Weights.Any()) return "No data to graph.";
            if (showFunc == null) showFunc = a => a.ToString();

            var normalized = Importance.Normalize(CompactUnordered(itemProbs, showFunc));
            var sb = new StringBuilder();
            var display = normalized.Weights.Select(ip => new Tuple<string, int, Prob>(showFunc(ip.Item), (int) Math.Floor(ip.Prob.Value * scale), ip.Prob));
            var maxWidth = display.Select(d => d.Item1.Length).Max();
            foreach (var line in display)
            {
                var item = line.Item1;
                sb.AppendLine($"{item.PadLeft(maxWidth)} {line.Item3,5} {Bar(line.Item2)}");
            }
            return sb.ToString();
        }
    }
}

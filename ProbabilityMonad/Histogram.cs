using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Print a bucket
        /// </summary>
        /// <param name="buckets"></param>
        /// <returns></returns>
        internal static string ShowBuckets(IEnumerable<Bucket> buckets)
        {
            var sb = new StringBuilder();
            foreach (var bucket in buckets)
            {
                var barBuilder = new StringBuilder();
                for (var i = 0; i < bucket.BarSize; i++)
                {
                    barBuilder.Append("#");
                }
                sb.AppendLine($"{bucket.Min:N2}\tto\t{bucket.Max:N2}\t{barBuilder.ToString()}");
            }
            sb.AppendLine("");
            return sb.ToString();
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



        public static string Weighted(IEnumerable<ItemProb<double>> nums, int numBuckets = 10)
        {
            if (!nums.Any()) return "No data to graph.";

            var sorted = nums.OrderBy(x => x.Item);
            var min = sorted.First().Item;
            var max = sorted.Last().Item;
            var bucketList = MakeBucketList(min, max, numBuckets);

            var totalMass = Sum(ip => ip.Prob.Value, sorted);
            foreach (var bucket in bucketList)
            {
                bucket.WeightedValues.AddRange(sorted.Where(x => x.Item >= bucket.Min && x.Item < bucket.Max));
                bucket.BarSize = (int) Math.Floor(bucket.WeightedValues.Select(x => x.Prob.Value).Sum() / totalMass * 40);
                var hasInfinity = bucket.WeightedValues.FirstOrDefault(ip => Double.IsInfinity(ip.Prob.Value)) != null;
                if (hasInfinity) bucket.BarSize = 10;
            }
            return ShowBuckets(bucketList);
        }

        public static string Unweighted(IEnumerable<double> nums, int numBuckets = 10)
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
                bucket.BarSize = (int) Math.Floor((double)bucket.Values.Count() / sorted.Count() * 40);
            }
            return ShowBuckets(bucketList);
        }
    }
}

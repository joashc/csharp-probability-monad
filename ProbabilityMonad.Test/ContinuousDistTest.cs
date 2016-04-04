using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProbabilityMonad;
using System.Diagnostics;
using static ProbabilityMonad.Base;
using System.Linq;
using System.Collections.Generic;

namespace CSharpProbabilityMonad.Test
{
    [TestClass]
    public class ContinuousDistTest
    {
        [TestMethod]
        public void NormalTest()
        {
            var normalDist = from n in Normal(0, 1)
                             from n2 in Normal(0, 1)
                             select $"{n}, {n2}";
        }

        /// <summary>
        /// Param class
        /// </summary>
        internal class Param
        {
            public double a { get; }
            public double b { get; }
            public Param(double a, double b)
            {
                this.a = a;
                this.b = b;
            }
        }

        /// <summary>
        /// Point class
        /// </summary>
        internal class Point
        {
            public double x { get; }
            public double y { get; }
            public Point(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [TestMethod]
        public void LinReg()
        {
            // Define a prior with heavy tails
            var prior = from a in Normal(5, 30)
                        from b in Normal(1, 30)
                        select new Param(a, b);

            // Define likelihood function
            Func<Dist<Param>, Point, Dist<Param>>
            linRegPoint = (dist, point) =>
            {
                return from cond in Condition(param =>
                            Pdf(NormalC(param.a * point.x + param.b, 1), point.y), dist)
                       select cond;
            };

            // Diameter of sand granules vs slope on beach
            // from http://college.cengage.com/mathematics/brase/understandable_statistics/7e/students/datasets/slr/frames/frame.html
            // y = 17.16x - 2.48 
            var points = new List<Point> {
                new Point(0.1700000018,0.6299999952),
                new Point(0.1899999976,0.6999999881),
                new Point(0.2199999988,0.8199999928),
                new Point(0.2349999994,0.8799999952),
                new Point(0.2349999994,1.149999976),
                new Point(0.3000000119,1.5),
                new Point(0.349999994,4.400000095),
                new Point(0.4199999869,7.300000191),
                new Point(0.8500000238,11.30000019),
            };

            // Create the linear regression, but don't do any inference yet
            var linReg = points.Aggregate(prior, linRegPoint, d => d);

            // Basically do importance sampling using the prior
            var samples = Enumerable.Range(0, 1000).Select(s => linReg.Prior().Sample());
            var posteriorA = samples.Select(sample => ItemProb(sample.Item.a, sample.Prob));
            var posteriorB = samples.Select(sample => ItemProb(sample.Item.b, sample.Prob));

            // Graph the results
            Debug.WriteLine("Posterior distribution of a:");
            Debug.WriteLine(Histogram.Weighted(posteriorA, 20));

            Debug.WriteLine("Posterior distribution of b:");
            Debug.WriteLine(Histogram.Weighted(posteriorB, 20));
        }

        [TestMethod]
        public void ThreeRolls()
        {
            var die = UniformF(1, 2, 3, 4, 5, 6);
            var threeRolls = from roll1 in die
                             from roll2 in die
                             from roll3 in die
                             select new List<int>() { roll1, roll2, roll3 };
            var samp3Roll = threeRolls.ToSampleDist();
            var sample = samp3Roll.Sample();
        }
    }
}

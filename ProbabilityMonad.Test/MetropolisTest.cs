using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ProbabilityMonad.Base;
using ProbabilityMonad;

namespace ProbabilityMonad.Test
{
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
    [TestClass]
    public class MetropolisTest
    {
        [TestMethod]
        public void MetropolisHastings()
        {
            // Define a prior with heavy tails
            var prior = from a in Primitive(Normal(20, 10))
                        from b in Primitive(Normal(1, 10))
                        select new Param(a, b);

            // Define likelihood function
            Func<Dist<Param>, Point, Dist<Param>>
            linRegPoint = (dist, point) =>
            {
                return from cond in Condition(param =>
                            Pdf(Normal(param.a * point.x + param.b, 1), point.y), dist)
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

            Func<Param, Prob> likelihood = param =>
            {
                return points.Aggregate(Prob(1), (pr, point) => pr.Mult(Pdf(Normal(param.a * point.x + param.b, 1), point.y)));
            };

            // Create the linear regression, but don't do any inference yet
            var linReg = points.Aggregate(prior, linRegPoint, d => d);
            var chain = ProbabilityMonad.MetropolisHastings.MHPrior(linReg, 20000);
            var sampled = chain.Sample().Skip(10000);

            var pimh = PIMH.Run(30, 3000, linReg);
            var samp = pimh.Sample();



            var posteriorA = from outer in samp
                             from inner in outer
                             select ItemProb(inner.Item.a, inner.Prob);

            var posteriorB = from outer in samp
                             from inner in outer
                             select ItemProb(inner.Item.b, inner.Prob);
            Debug.WriteLine(Histogram.Unweighted(posteriorA.Where(ip => Double.IsInfinity(ip.Prob.Value)).Select(ip => ip.Item)));
            Debug.WriteLine("b");
            Debug.WriteLine(Histogram.Unweighted(posteriorB.Where(ip => Double.IsInfinity(ip.Prob.Value)).Select(ip => ip.Item)));


        }
    }
}

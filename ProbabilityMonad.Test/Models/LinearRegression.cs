using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbCSharp.ProbBase;

namespace ProbCSharp.Test.Models
{
    public class Param
    {
        public double a { get; }
        public double b { get; }
        public Param(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public override string ToString()
        {
            return $"Param ({a}, {b})";
        }
    }

    public class Point
    {
        public double x { get; }
        public double y { get; }
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public static class LinearRegression
    {
        public static Func<double, double, Point> Point = (x, y) => new Point(x, y);

        /// <summary>
        /// Perfectly linear datapoints
        /// y = 4x + 2
        /// </summary>
        public static List<Point> LinearData = new List<Point> {
                Point(1,6), Point(2,10), Point(3,14), Point(4,16), Point(5,22),
                Point(6,26), Point(7,30), Point(8,34), Point(9,38), Point(10,42),
                Point(11,46), Point(12,50), Point(13,54), Point(14,58), Point(15,62),
                Point(16,66), Point(17,70), Point(18,74), Point(19,78), Point(20,82),
                Point(21,86), Point(22,90), Point(23,94), Point(24,98), Point(25,102)
            };

        /// <summary>
        /// Diameter of sand granules vs slope on beach 
        /// from http://college.cengage.com/mathematics/brase/understandable_statistics/7e/students/datasets/slr/frames/frame.html
        /// y = 17.16x - 2.48 
        /// </summary>
        public static List<Point> BeachSandData = new List<Point> {
                Point(0.1700000018,0.6299999952), Point(0.1899999976,0.6999999881),
                Point(0.2199999988,0.8199999928), Point(0.2349999994,0.8799999952),
                Point(0.2349999994,1.149999976), Point(0.3000000119,1.5),
                Point(0.349999994,4.400000095), Point(0.4199999869,7.300000191),
                Point(0.8500000238,11.30000019),
            };

        /// <summary>
        /// Represents posterior distribution after updating on one point
        /// </summary>
        public static Dist<Param> LinearRegressionPoint(Dist<Param> prior, Point point)
        {
            return Condition(param => Pdf(NormalC(param.a * point.x + param.b, 10), point.y), prior);
        }

        /// <summary>
        /// Represents posterior distribution after updating on a list of points
        /// </summary>
        public static Dist<Param> CreateLinearRegression(Dist<Param> prior, List<Point> points)
        {
            return points.Aggregate(prior, LinearRegressionPoint);
        }
    }
}

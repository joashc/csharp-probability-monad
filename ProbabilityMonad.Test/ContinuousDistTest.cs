using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProbabilityMonad;
using System.Diagnostics;
using static ProbabilityMonad.Distributions;
using static ProbabilityMonad.Base;
using static ProbabilityMonad.ProbabilityFunctions;
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
            var linear = from a in Normal(0, 1)
                         from b in Normal(0, 1)
                         select new Param(a, b);

            var linny = from a in new Primitive<double>(Normal(0, 1))
                        from b in new Primitive<double>(Normal(0, 2))
                        select new Param(a, b);

            //Func<ContDist<Param>, Point, ContDist<Param>>
            //linRegPoint = (dist, point) =>
            //    dist.ConditionSoft(param => NormalPdf(param.a * point.x + param.b, 1, point.y));



        }

        [TestMethod]
        public void ThreeRolls()
        {
            var die = UniformD(1, 2, 3, 4, 5, 6);
            var threeRolls = from roll1 in die
                             from roll2 in die
                             from roll3 in die
                             select new List<int>() { roll1, roll2, roll3 };
            var samp3Roll = threeRolls.ToSampleDist();
            var sample = samp3Roll.Sample();
        }
    }
}

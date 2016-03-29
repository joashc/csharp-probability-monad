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

        [TestMethod]
        public void LinReg()
        {
            var linear = from a in Normal(0, 1)
                         from b in Normal(0, 1)
                         select new Tuple<double, double>(a, b);
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

            Assert.AreEqual($"{sample[0]}, {sample[1]}, {sample[2]}", "something");
        }
    }
}

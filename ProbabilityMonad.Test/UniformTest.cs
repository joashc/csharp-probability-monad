using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ProbabilityMonad;
using static ProbabilityMonad.Base;
using System.Linq;
using System.Collections.Generic;

namespace ProbabilityMonad.Test
{
    [TestClass]
    public class UniformTests
    {
        [TestMethod]
        public void CoinFlip_SumsToOne()
        {
            var coin = UniformD(1, 2);
            Assert.AreEqual(1, coin.Distribution.Select(p => p.Prob.Value).Sum());
        }

        [TestMethod]
        public void SingleUniformDie_ProbIsOne()
        {
            var die = UniformD(1, 2, 3, 4, 5, 6);
            Assert.AreEqual("16.6%", die.ProbOf(roll => roll == 1).ToString());
        }

        [TestMethod]
        public void ThreeDice_AtLeast2Odd()
        {
            var die = UniformD(1, 2, 3, 4, 5);
            var threeRolls = from roll1 in die
                             from roll2 in die
                             from roll3 in die
                             select new List<int>() { roll1, roll2, roll3 };

            var atLeast2Odd = threeRolls.ProbOf(s => s.Where(i => i % 2 == 1).Count() >= 2);
            Assert.AreEqual("64.8%", atLeast2Odd.ToString());
        }

        [TestMethod]
        public void ThreeDice_SumTo8()
        {
            var die = UniformD(1, 2, 3, 4, 5, 6);
            var threeRolls = from roll1 in die
                             from roll2 in die
                             from roll3 in die
                             select roll1 + roll2 + roll3;

            var pSumTo8 = threeRolls.ProbOf(s => s == 8);
            Assert.AreEqual("9.7%", pSumTo8.ToString());
        }

        [TestMethod]
        public void FreeMonad()
        {
            var y =
                from a in new Primitive<double>(Normal(0, 1))
                from b in new Primitive<double>(Normal(0, 1))
                from cond in new Conditional<double>(a => Prob(0.2), new Pure<double>(b))
                select cond;


            var prior = y.Prior();


            var sample = prior.Sample();

        }

    }
}


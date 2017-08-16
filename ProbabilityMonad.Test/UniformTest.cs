using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ProbCSharp.Test.Models;
using static ProbCSharp.ProbBase;
using System.Linq;
using System.Collections.Generic;

namespace ProbCSharp.Test
{
    [TestClass]
    public class UniformTests
    {
        [TestMethod]
        public void CoinFlip_SumsToOne()
        {
            var coin = UniformF(1, 2);
            Assert.AreEqual(1, coin.Explicit.Weights.Select(p => p.Prob.Value).Sum());
        }

        [TestMethod]
        public void SingleUniformDie_ProbIsOne()
        {
            var die = UniformF(1, 2, 3, 4, 5, 6);
            Debug.WriteLine(Histogram.Finite(die.Explicit));
            Assert.AreEqual("16.7%", die.ProbOf(roll => roll == 1).ToString());
        }

        [TestMethod]
        public void ThreeDice_AtLeast2Odd()
        {
            var die = UniformF(1, 2, 3, 4, 5);
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
            var die = UniformF(1, 2, 3, 4, 5, 6);
            var threeRolls = from roll1 in die
                             from roll2 in die
                             from roll3 in die
                             select roll1 + roll2 + roll3;

            var pSumTo8 = threeRolls.ProbOf(s => s == 8);
            var pSumTo8x = Dice.DieExact(3).ProbOf(s => s == 8);
            Debug.WriteLine(threeRolls.Histogram());
            Debug.WriteLine(Dice.DieExact(3).Histogram());
            Assert.AreEqual("9.72%", pSumTo8.ToString());
            Assert.AreEqual("9.72%", pSumTo8x.ToString());
        }

        [TestMethod]
        public void IndependentDieRolls()
        {
            // Exact distribution of sum of three three rolls
            var threeRollsExact = Dice.DieExact(3);
            Debug.WriteLine(threeRollsExact.Histogram());

            // Stochastic form
            var threeRolls = Dice.Die(3).SampleNParallel(10).Select(i => (double) i);
            Debug.WriteLine(Histogram.Unweighted(threeRolls, 15));
        }

        [TestMethod]
        public void ConditionalDie()
        {
            // Exact posterior conditioned die
            var twoRollsExact = Dice.ConditionalDieExact(2);
            Debug.WriteLine(twoRollsExact.Histogram());

            var twoRolls = Dice.ConditionalDie(2);
            var priorWeights = twoRolls.WeightedPrior().SampleNParallel(100).Select(ip => ItemProb((double)ip.Item, ip.Prob));
            Debug.WriteLine(Histogram.Weighted(priorWeights, scale: 60));
        }

    }
}


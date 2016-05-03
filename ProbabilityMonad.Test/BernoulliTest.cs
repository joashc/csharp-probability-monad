using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProbCSharp;
using System.Diagnostics;
using static ProbCSharp.ProbBase;
using System.Linq;
using System.Collections.Generic;
using static ProbCSharp.Test.Models.CoinExt;
using ProbCSharp.Test.Models;

namespace ProbCSharp.Test
{
    [TestClass]
    public class BernoulliTest
    {
        [TestMethod]
        public void CoinFlip_SumsToOne()
        {
            var coin = BernoulliF(Prob(0.5));
            Assert.AreEqual(1, coin.Explicit.Weights.Select(p => p.Prob.Value).Sum());
        }

        [TestMethod]
        public void CoinToss_InferWeighted()
        {
            // We put a prior on the coin being fair with Pr(0.8).
            // If it's biased, we put a prior on its weight with a Beta distribution.
            var prior = from isFair in Bernoulli(Prob(0.8))
                        from headsProb in isFair ? Return(0.5) : Beta(5, 1)
                        select headsProb;

            // Some coinflips from a fair coin
            var fair = new List<Coin> { Heads, Tails, Tails, Heads, Heads, Tails, Heads, Tails };

            // The posterior weight distribution given these fair flips
            var posteriorWeight = FlipsUpdate(fair, prior);

            // Use a weighted prior to infer the posterior
            var inferredWeight = posteriorWeight.WeightedPrior().SampleNParallel(100);
            Debug.WriteLine(Histogram.Weighted(inferredWeight));

            // Do the same with a biased coin
            var biased = new List<Coin> { Tails, Tails, Tails, Tails, Tails, Tails, Heads, Tails };
            var inferredBiasedWeight = FlipsUpdate(biased, prior).WeightedPrior().SampleNParallel(100);
            Debug.WriteLine(Histogram.Weighted(inferredBiasedWeight));
        }

        [TestMethod]
        public void CoinToss_Exact()
        {
            // There's an 80% chance we have a fair coin and a 20% chance of a biased coin
            // with an 80% chance of tails.
            var prior = from isFair in BernoulliF(Prob(0.7))
                             from headsProb in isFair ? UniformF(0.5) : UniformF(0.2)
                             select headsProb;
            Debug.WriteLine(prior.Histogram(scale:50));

            // Some coinflips from a fair coin
            var fair = new List<Coin> { Heads, Tails, Tails, Heads, Heads, Tails, Heads, Tails };

            // The posterior weight distribution given these fair flips
            var exactPosterior = FlipsUpdateExact(fair, prior);
            Debug.WriteLine(exactPosterior.Histogram(scale:50));

            // Do the same with a biased coin
            var biased = new List<Coin> { Tails, Tails, Tails, Tails, Tails, Tails, Heads, Tails };
            var biasedPosterior = FlipsUpdateExact(biased, prior);
            Debug.WriteLine(biasedPosterior.Histogram());
        }

        enum Location { Pub, Starbucks };

        [TestMethod]
        public void Meet()
        {

            Func<Prob, FiniteDist<Location>>
            location = pubPref => from visitPub in BernoulliF(pubPref)
                                  select visitPub ? Location.Pub : Location.Starbucks;

            var meetByChance = from amy in location(Prob(0.6))
                               from bob in location(Prob(0.6))
                               select new Tuple<Location, Location>(amy, bob);

            var amyAtPub = meetByChance.ProbOf(t => t.Item1 == Location.Pub);
            var meet = meetByChance.ProbOf(t => t.Item1 == t.Item2);
            Debug.WriteLine(amyAtPub);
            Debug.WriteLine(meet);

            var meetAtPub = meetByChance.ConditionHard(t => t.Item1 == t.Item2);

            var prPub = meetAtPub.ProbOf(t => t.Item1 == Location.Pub);
            Debug.WriteLine(prPub);
        }
    }
}


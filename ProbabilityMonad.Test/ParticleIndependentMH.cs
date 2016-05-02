using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ProbCSharp.ProbBase;
using ProbCSharp.Test.Models;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using static ProbCSharp.Test.Models.LinearRegression;
using static ProbCSharp.Test.Models.BayesPointMachine;
using static ProbCSharp.Test.Models.HiddenMarkovModel;

namespace ProbCSharp.Test
{
    [TestClass]
    public class ParticleIndependentMH
    {
        [TestMethod]
        public void Pimh_Dice()
        {
            // Exact posterior conditioned die
            var twoRollsExact = Dice.ConditionalDieExact(2);
            Debug.WriteLine(twoRollsExact.Histogram());

            // Posterior inferred with SMC
            var twoRolls = Dice.ConditionalDie(2);
            var pimhPosterior = Pimh.Run(100, 1000, twoRolls).Sample();
            var weights = from samples in pimhPosterior
                          from sample in samples
                          select sample;
            var klDivergence = KullbackLeibner.KLDivergence(Samples(weights), twoRollsExact, d => d);
            Debug.WriteLine($"KL divergence: {klDivergence}");
            Assert.IsTrue(klDivergence < 0.5);

            var doubleWeights = weights.Select(ip => ItemProb((double)ip.Item, ip.Prob));
            Debug.WriteLine(Histogram.Weighted(doubleWeights, scale: 60));
        }

        [TestMethod]
        public void Pimh_LinReg()
        {
            var prior = from a in Normal(0, 100)
                        from b in Normal(0, 100)
                        select new Param(a, b);

            var sandLinReg = CreateLinearRegression(prior, BeachSandData);

            var pimhLinReg = Pimh.Run(100, 1000, sandLinReg).Sample();

            var paramA = from samples in pimhLinReg
                         from aProb in samples.MapSample(param => param.a)
                         select aProb;

            var paramB = from samples in pimhLinReg
                         from bProb in samples.MapSample(param => param.b)
                         select bProb;

            Debug.WriteLine(Histogram.Weighted(paramA));
            Debug.WriteLine(Histogram.Weighted(paramB));
        }

        [TestMethod]
        public void Pimh_HiddenMarkov()
        {
            var hmmModel = Hmm(ObservedHmmData1);

            var smcHmmSamples = Pimh.Run(100, 100, hmmModel).Sample();

            var topSamples = Samples((from samples in smcHmmSamples
                                      from sample in samples.Weights
                                      select sample)
                .GroupBy(ip => ShowLatentList(ip.Item))
                .OrderByDescending(group => group.First().Prob.Value)
                .Take(5).Select(g => g.First()));

            Debug.WriteLine(Histogram.Finite(topSamples, ShowLatentList));
        }

        [TestMethod]
        public void Pimh_BayesPointMachine()
        {
            var posterior = BpmUpdate(BpmPrior, BpmObserved);
            var samples = Samples(Pimh.Run(500, 50, posterior).SampleNParallel(100).SelectMany(d => d).SelectMany(d => d));

            var approxPosterior = CategoricalF(samples.Normalize());

            Debug.WriteLine(BpmPredict(approxPosterior, Person1));
            Debug.WriteLine(BpmPredict(approxPosterior, Person2));
            Debug.WriteLine(BpmPredict(approxPosterior, Person3));
        }
    }
}

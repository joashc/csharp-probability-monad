using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ProbabilityMonad.Test.Models.BayesPointMachine;
using static ProbabilityMonad.Base;
using ProbabilityMonad;
using ProbabilityMonad.Test.Models;

namespace ProbabilityMonad.Test
{

    [TestClass]
    public class MetropolisTest
    {
        [TestMethod]
        public void Metropolis_ConditionalDice()
        {
            var dice = Dice.ConditionalDie(2);

            var mh = MetropolisHastings.MHPrior(dice, 1000);
            var samples = mh.SampleNParallel(100);

            var sumDist = from outer in samples
                          from inner in outer
                          select (double) inner;

            Debug.WriteLine(Histogram.Unweighted(sumDist));
        }

        [TestMethod]
        public void Metropolis_LinReg()
        {
            // Define a prior with heavy tails
            var prior = from a in Normal(0, 100)
                        from b in Normal(0, 100)
                        select new Param(a, b);

            var linReg = LinearRegression.CreateLinearRegression(prior, LinearRegression.BeachSandData);
            var mh = MetropolisHastings.MHPrior(linReg, 1000);
            var samples = mh.SampleNParallel(100);

            var paramA = from outer in samples
                         from inner in outer
                         select inner.a;

            var paramB = from outer in samples
                         from inner in outer
                         select inner.b;

            Debug.WriteLine(Histogram.Unweighted(paramA));
            Debug.WriteLine(Histogram.Unweighted(paramB));
        }

        [TestMethod]
        public void Metropolis_BayesPointMachine()
        {
            var posterior = BpmUpdate(BpmPrior, BpmObserved);
            var samples = Samples(MetropolisHastings.MHPrior(posterior, 100)
                                            .SampleNParallel(10000)
                                            .SelectMany(d => d)
                                            .Select(d => ItemProb(d, Prob(1))));

            var approxPosterior = CategoricalF(samples.Normalize());

            Debug.WriteLine(BpmPredict(approxPosterior, Person1));
            Debug.WriteLine(BpmPredict(approxPosterior, Person2));
            Debug.WriteLine(BpmPredict(approxPosterior, Person3));
        }
    }
}

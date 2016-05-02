using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProbabilityMonad;
using System.Diagnostics;
using static ProbabilityMonad.Base;
using static ProbabilityMonad.Test.Models.IndianGpaModel;
using ProbabilityMonad.Test.Models;
using System.Linq;
using System.Collections.Generic;

namespace ProbabilityMonad.Test
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

            var hist = from n in Normal(0, 1000)
                       select n;

            var samples = hist.SampleN(1000);
            Debug.WriteLine(Histogram.Unweighted(samples));
        }

        [TestMethod]
        public void LinReg()
        {
            // Define a prior with heavy tails
            var prior = from a in Normal(5, 10)
                        from b in Normal(1, 10)
                        select new Param(a, b);

            // Create the linear regression, but don't do any inference yet
            var linReg = LinearRegression.CreateLinearRegression(prior, LinearRegression.LinearData);

            // Basically do importance sampling using the prior
            var samples = linReg.Prior().SampleNParallel(1000);

            var posteriorA = samples.Select(sample => ItemProb(sample.Item.a, sample.Prob));
            var posteriorB = samples.Select(sample => ItemProb(sample.Item.b, sample.Prob));

            // Graph the results
            Debug.WriteLine("Posterior distribution of a:");
            Debug.WriteLine(Histogram.Weighted(posteriorA, 20, 2));

            Debug.WriteLine("Posterior distribution of b:");
            Debug.WriteLine(Histogram.Weighted(posteriorB, 20, 2));
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

        [TestMethod]
        public void IndianGpaTest()
        {
            var samples = AmericanGpa.SampleN(10000);
            Debug.WriteLine(Histogram.Unweighted(samples, numBuckets:40, scale:500));

            var indianSamples = IndianGpa.SampleN(10000);
            Debug.WriteLine(Histogram.Unweighted(indianSamples, numBuckets:40, scale:500));

            var combined = from indianGpa in Independent(IndianGpa)
                           from americanGpa in Independent(AmericanGpa)
                           from isAmerican in Bernoulli(0.25)
                           from gpa in isAmerican ? americanGpa : indianGpa
                           select gpa;
            var combinedSamples = combined.SampleN(10000);

            Debug.WriteLine(Histogram.Unweighted(combinedSamples, numBuckets:40, scale:500));
        }
    }
}

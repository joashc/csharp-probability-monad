using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using ProbabilityMonad.Test.Models;
using static ProbabilityMonad.Base;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProbabilityMonad.Test
{
    /// <summary>
    /// Tests parallel evaluation of distributions.
    /// These tests will *not* pass on single core machines!
    /// </summary>
    [TestClass]
    public class ParallelTest
    {
        // Initialize common objects
        private static int waitMillis = 100;
        private Func<int, int, Dist<int>> Add = (a, b) => Return(a + b);
        private Dist<int> LaggyDist = Primitive(new ContDistImpl<int>(() =>
        {
            Thread.Sleep(waitMillis);
            return 5;
        }));

        /// <summary>
        /// This compares the time it takes to sample from a slow distribution
        /// using parallel and sequential evaluation.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void ParallelSampler()
        {
            // Define sequentially evaluated distribution
            var seqSum = from first in LaggyDist
                         from second in LaggyDist
                         select first + second;

            // Time sequential execution
            var seqTimer = new Stopwatch();
            seqTimer.Start();
            var seqTotal = seqSum.RunParallel(new ParallelSampler<int>());
            seqTimer.Stop();
            var seqTime = seqTimer.ElapsedMilliseconds;


            // Define parallelized distribution
            var parallelSum = from first in Independent(LaggyDist)
                              from second in Independent(LaggyDist)
                              from pair in RunIndependent(first, second)
                              select pair.Item1 + pair.Item2;

            // Time parallel execution
            var parTimer = new Stopwatch();
            parTimer.Start();
            var parTotal = parallelSum.RunParallel(new ParallelSampler<int>());
            parTimer.Stop();
            var parTime = parTimer.ElapsedMilliseconds;


            // Check both answers are correct
            Assert.AreEqual(10, parTotal);
            Assert.AreEqual(10, seqTotal);

            // Check parallelization
            Debug.WriteLine($"Sequential: {seqTime}ms, Parallel: {parTime}ms");
            Assert.IsTrue(seqTime > parTime + (waitMillis - 10));
        }

        /// <summary>
        /// Tests parallization of sampling multiple times from a distribution
        /// </summary>
        [TestMethod]
        public void ParallelNSampler()
        {
            var prior = from a in Normal(0, 100)
                        from b in Normal(0, 100)
                        select new Param(a, b);

            var smc = LinearRegression.CreateLinearRegression(prior, LinearRegression.BeachSandData).SmcStandard(100);

            var sTimer = new Stopwatch();
            sTimer.Start();
            var serial = from s1 in smc
                         from s2 in smc
                         from s3 in smc
                         select new Tuple<Samples<Param>, Samples<Param>, Samples<Param>>(s1, s2, s3);
            var result = serial.SampleN(100).ToList();
            sTimer.Stop();

            var pTimer = new Stopwatch();
            pTimer.Start();
            var parallel = from s1 in Independent(smc)
                           from s2 in Independent(smc)
                           from s3 in Independent(smc)
                           from triple in RunIndependent(s1, s2, s3)
                           select triple;
            var thing = parallel.SampleNParallel(100).ToList();
            pTimer.Stop();


            Debug.WriteLine($"parallel: {pTimer.ElapsedMilliseconds}ms, serial: {sTimer.ElapsedMilliseconds}ms");
            Assert.IsTrue(pTimer.ElapsedMilliseconds < sTimer.ElapsedMilliseconds);
        }

        /// <summary>
        /// Alternative syntax for parallel distributions
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void RunParallelDirectly()
        {
            // Define parallelized distribution
            var parallelSum = from pair in RunIndependent(LaggyDist, LaggyDist)
                              select pair.Item1 + pair.Item2;

            // Time parallel execution
            var timer = new Stopwatch();
            timer.Start();
            var total = parallelSum.RunParallel(new ParallelSampler<int>());
            timer.Stop();
            var time = timer.ElapsedMilliseconds;


            // Check both answers are correct
            Assert.AreEqual(10, total);

            // Check parallelization
            Assert.IsTrue(time < waitMillis * 2);
        }

        /// <summary>
        /// Inference is still able to be performed on distributions
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void ParallelInference()
        {
            var parallelSum = from first in Independent(LaggyDist)
                              from second in Independent(LaggyDist)
                              from pair in RunIndependent(first, second)
                              select pair.Item1 + pair.Item2;

            var parTimer = new Stopwatch();
            parTimer.Start();
            var total = parallelSum.Prior().RunParallel(new ParallelSampler<ItemProb<int>>());
            parTimer.Stop();

            Assert.AreEqual(total.Item, 10);
            Assert.IsTrue(parTimer.ElapsedMilliseconds < 200);
        }

        /// <summary>
        /// Checks if three distributions can be run in parallel
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void ThreeInParallel()
        {
            Func<int, int, int, Dist<int>> Add3 = (a, b, c) => Return(a + b + c);

            // Define parallel distribution with three dists
            var parallelSum = from first in Independent(LaggyDist)
                              from second in Independent(LaggyDist)
                              from third in Independent(LaggyDist)
                              from triple in RunIndependent(first, second, third)
                              select triple.Item1 + triple.Item2 + triple.Item3;

            // Time parallel execution
            var parTimer = new Stopwatch();
            parTimer.Start();
            var parTotal = parallelSum.RunParallel(new ParallelSampler<int>());
            parTimer.Stop();
            var parTime = parTimer.ElapsedMilliseconds;

            // Check answer is correct
            Assert.AreEqual(15, parTotal);

            // Check parallelization
            Assert.IsTrue(parTime < (waitMillis * 3) - 10);
        }

        /// <summary>
        /// Tests composition of parallel distributions
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void ParallelComposition()
        {
            // Define parallel distribution with three dists
            var parallelSum = from first in Independent(LaggyDist)
                              from second in Independent(LaggyDist)
                              from pair in RunIndependent(first, second)
                              select pair.Item1 + pair.Item2;

            var composed = from sum1 in Independent(parallelSum)
                           from sum2 in Independent(parallelSum)
                           from pair in RunIndependent(sum1, sum2)
                           select pair.Item1 + pair.Item2;

            // Time parallel execution
            var timer = new Stopwatch();
            timer.Start();
            var total = composed.SampleParallel();
            timer.Stop();
            var time = timer.ElapsedMilliseconds;

            // Check answer is correct
            Assert.AreEqual(20, total);

            // Check parallelization
            Assert.IsTrue(time < (waitMillis * 4));
        }
    }
}

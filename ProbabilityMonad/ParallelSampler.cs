using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProbCSharp
{
    public static class ParallelSampleExtensions
    {
        /// <summary>
        /// Samples from a distribution 
        /// Samples from independent distributions in parallel
        /// This will throw an exception if the distribution contains any conditionals.
        /// </summary>
        public static A SampleParallel<A>(this Dist<A> dist)
        {
            return dist.RunParallel(new ParallelSampler<A>());
        }

        /// <summary>
        /// Draws n samples from a distribution in parallel
        /// Samples from independent distributions in parallel
        /// This will throw an exception if the distribution contains any conditionals.
        /// </summary>
        public static IEnumerable<A> SampleNParallel<A>(this Dist<A> dist, int n)
        {
            var results = Enumerable.Range(0, n).AsParallel().Select(_ => dist.SampleParallel());
            return results.AsEnumerable();
        }
    }

    /// <summary>
    /// Allows sampling from distributions in parallel
    /// </summary>
    public class ParallelSampler<A> : ParallelDistInterpreter<A, A>
    {
        private Task<T> StartTask<T>(Func<T> func)
        {
            return Task.Factory.StartNew(func);
        }

        public A Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            var x = dist.RunParallel(new ParallelSampler<B>());
            return bind(x).RunParallel(new ParallelSampler<A>());
        }

        public A Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            throw new ArgumentException("Cannot sample from conditional distribution.");
        }

        public A Primitive(PrimitiveDist<A> dist)
        {
            return dist.Sample();
        }

        public A Pure(A value)
        {
            return value;
        }

        public A Independent(Dist<A> independent)
        {
            return independent.RunParallel(new ParallelSampler<A>());
        }

        public A RunIndependent<B, C>(Dist<B> distB, Dist<C> distC, Func<B, C, Dist<A>> run)
        {
            var taskB = StartTask(() => distB.RunParallel(new ParallelSampler<B>()));
            var taskC = StartTask(() => distC.RunParallel(new ParallelSampler<C>()));
            Task.WaitAll(taskB, taskC);
            return run(taskB.Result, taskC.Result).RunParallel(new ParallelSampler<A>());
        }

        public A RunIndependent3<B, C, D>(Dist<B> distB, Dist<C> distC, Dist<D> distD, Func<B, C, D, Dist<A>> run)
        {
            var taskB = StartTask(() => distB.RunParallel(new ParallelSampler<B>()));
            var taskC = StartTask(() => distC.RunParallel(new ParallelSampler<C>()));
            var taskD = StartTask(() => distD.RunParallel(new ParallelSampler<D>()));
            Task.WaitAll(taskB, taskC, taskD);
            return run(taskB.Result, taskC.Result, taskD.Result).RunParallel(new ParallelSampler<A>());
        }
    }

}

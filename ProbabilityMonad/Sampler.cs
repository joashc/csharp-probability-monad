using System;
using System.Linq;
using System.Collections.Generic;

namespace ProbCSharp
{
    public static class SamplerExt
    {
        /// <summary>
        /// Samples from a distribution.
        /// This will throw an exception if the distribution contains any conditionals.
        /// </summary>
        public static A Sample<A>(this Dist<A> dist)
            => dist.Run(new Sampler<A>());

        /// <summary>
        /// Draws n samples from a distribution.
        /// This will throw an exception if the distribution contains any conditionals.
        /// </summary>
        public static IEnumerable<A> SampleN<A>(this Dist<A> dist, int n)
            => Enumerable.Range(0, n).Select(_ => dist.Sample());
    }

    /// <summary>
    /// Sample from a distribution
    /// </summary>
    public class Sampler<A> : DistInterpreter<A, A>
    {
        public DistInterpreter<C, Y> New<C, Y>()
            => new Sampler<C>() as DistInterpreter<C, Y>;

        A DistInterpreter<A, A>.Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            var x = dist.Run(new Sampler<B>());
            return bind(x).Run(new Sampler<A>());
        }

        /// <summary>
        /// All conditionals must be removed before sampling.
        /// </summary>
        A DistInterpreter<A, A>.Conditional(Func<A, Prob> lik, Dist<A> dist)
            => throw new ArgumentException("Cannot sample from conditional distribution.");

        A DistInterpreter<A, A>.Primitive(PrimitiveDist<A> dist)
            => dist.Sample();

        A DistInterpreter<A, A>.Pure(A value)
            => value;
    }

}

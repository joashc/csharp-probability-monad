using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProbCSharp
{
    /// <summary>
    /// Sampler extension methods
    /// </summary>
    public static class SamplerExt
    {
        public static A Sample<A>(this Dist<A> dist)
        {
            return dist.Run(new Sampler<A>());
        }

        public static IEnumerable<A> SampleN<A>(this Dist<A> dist, int n)
        {
            return Enumerable.Range(0, n).Select(_ => dist.Sample());
        }
    }

    /// <summary>
    /// Sample from a distribution
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Sampler<A> : DistInterpreter<A, A>
    {
        public DistInterpreter<C, Y> New<C, Y>()
        {
            return new Sampler<C>() as DistInterpreter<C, Y>;
        }

        A DistInterpreter<A, A>.Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            var x = dist.Run(new Sampler<B>());
            return bind(x).Run(new Sampler<A>());
        }

        /// <summary>
        /// All conditionals must be removed before sampling.
        /// </summary>
        /// <param name="lik"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        A DistInterpreter<A, A>.Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            throw new ArgumentException("Cannot sample from conditional distribution.");
        }

        A DistInterpreter<A, A>.Primitive(ContDist<A> dist)
        {
            return dist.Sample();
        }

        A DistInterpreter<A, A>.Pure(A value)
        {
            return value;
        }
    }

}

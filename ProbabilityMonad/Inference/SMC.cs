using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    /// <summary>
    /// SMC Extension methods
    /// </summary>
    public static class SmcExtensions
    {

        /// <summary>
        /// SMC that just discards pseudo-marginal likelihood
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="n"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<Samples<A>> SmcStandard<A>(this Dist<A> dist, int n)
        {
            return dist.Run(new Smc<A>(n)).Run(new Prior<Samples<A>>());
        }

        /// <summary>
        /// SMC that does importance sampling
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <param name="numSamples">Number of importance samples</param>
        /// <param name="numParticles"></param>
        /// <returns></returns>
        public static Dist<Samples<A>> SmcMultiple<A>(this Dist<A> dist, int numSamples, int numParticles)
        {
            return dist.Run(new Smc<A>(numParticles)).ImportanceSamples(numSamples).Select(Importance.Flatten);
        }
    }

    /// <summary>
    /// Sequential Monte Carlo
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Smc<A> : DistInterpreter<A, Dist<Samples<A>>>
    {
        public int numParticles;
        public Smc(int numParticles)
        {
            this.numParticles = numParticles;
        }

        public Dist<Samples<A>> Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            return from ps in dist.Run(new Smc<B>(numParticles))
                   let unzipped = ps.Unzip()
                   from ys in unzipped.Item1.Select(bind).Sequence()
                   select Samples(ys.Zip(unzipped.Item2, ItemProb));
        }

        public Dist<Samples<A>> Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            var updated = new Conditional<Samples<A>>(
                samples => samples.SumProbs(),

                from ps in dist.Run(new Smc<A>(numParticles))
                select ps.Select(ip => ItemProb(ip.Item, lik(ip.Item).Mult(ip.Prob)))
           );

            return updated.Select(Importance.Normalize)
                          .SelectMany(Importance.Resample);
        }

        public DistInterpreter<B, Y> New<B, Y>()
        {
            return new Smc<B>(numParticles) as DistInterpreter<B, Y>;
        }

        public Dist<Samples<A>> Primitive(ContDist<A> dist)
        {
            var d = new Primitive<ItemProb<A>>(new ContDistImpl<ItemProb<A>>(() => ItemProb(dist.Sample(), Prob(1))));
            return Enumerable.Repeat(d, numParticles).Sequence().Select(Samples);
        }

        public Dist<Samples<A>> Pure(A value)
        {
            var d = Return(ItemProb(value, Prob(1)));
            return Enumerable.Repeat(d, numParticles).Sequence().Select(Samples);
        }
    }
}

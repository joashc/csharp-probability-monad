using System;
using System.Linq;
using static ProbCSharp.ProbBase;

namespace ProbCSharp
{
    // SMC Extension methods
    public static class SmcExtensions
    {

        /// <summary>
        /// SMC that discards pseudo-marginal likelihoods
        /// </summary>
        public static Dist<Samples<A>> SmcStandard<A>(this Dist<A> dist, int n)
            => dist.Run(new Smc<A>(n)).Run(new Prior<Samples<A>>());

        /// <summary>
        /// SMC that does importance sampling
        /// </summary>
        public static Dist<Samples<A>> SmcMultiple<A>(this Dist<A> dist, int numSamples, int numParticles)
            => dist.Run(new Smc<A>(numParticles)).ImportanceSamples(numSamples).Select(Importance.Flatten);
    }

    /// <summary>
    /// Sequential Monte Carlo
    /// </summary>
    public class Smc<A> : DistInterpreter<A, Dist<Samples<A>>>
    {
        public int numParticles;
        public Smc(int numParticles)
            => this.numParticles = numParticles;

        public Dist<Samples<A>> Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
            => from ps in dist.Run(new Smc<B>(numParticles))
                let unzipped = ps.Unzip()
                from ys in unzipped.Item1.Select(bind).Sequence()
                select Samples(ys.Zip(unzipped.Item2, ItemProb));

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
            => new Smc<B>(numParticles) as DistInterpreter<B, Y>;

        public Dist<Samples<A>> Primitive(PrimitiveDist<A> dist)
        {
            var d = new Primitive<ItemProb<A>>(new SampleDist<ItemProb<A>>(() => ItemProb(dist.Sample(), Prob(1))));
            return Enumerable.Repeat(d, numParticles).Sequence().Select(Samples);
        }

        public Dist<Samples<A>> Pure(A value)
        {
            var d = Return(ItemProb(value, Prob(1)));
            return Enumerable.Repeat(d, numParticles).Sequence().Select(Samples);
        }
    }
}

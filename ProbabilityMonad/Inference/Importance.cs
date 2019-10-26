using System.Linq;
using static ProbCSharp.ProbBase;

namespace ProbCSharp
{
    // Extension methods for importance sampling
    public static class ImportanceExt
    {
        /// <summary>
        /// Importance sample with given number of samples
        /// </summary>
        public static Dist<Samples<A>> ImportanceSamples<A>(this Dist<A> dist, int numSamples)
            => Importance.ImportanceSamples(numSamples, dist);
    }

    /// <summary>
    /// Methods for importance sampling
    /// </summary>
    public static class Importance
    {
        /// <summary>
        /// Resamples from a group of samples, with the probability of
        /// a particular value given by the number of times it appears in the sample.
        /// </summary>
        public static Dist<Samples<A>> Resample<A>(Samples<A> samples)
        {
            var resamples = samples.Select(s => ItemProb(s, Prob(1)));
            var dist = Primitive(new FiniteDist<ItemProb<A>>(resamples).ToSampleDist());
            return Enumerable.Repeat(dist, samples.Weights.Count()).Sequence().Select(Samples);
        }

        /// <summary>
        /// Flattens nested samples
        /// </summary>
        public static Samples<A> Flatten<A>(Samples<Samples<A>> samples)
            => Samples(from outer in Normalize(samples).Weights
                from inner in outer.Item.Weights
                select ItemProb(inner.Item, inner.Prob.Mult(outer.Prob)));

        /// <summary>
        /// Performs importance sampling on a distribution
        /// </summary>
        /// <returns>A distribution of importance samples</returns>
        public static Dist<Samples<A>> ImportanceSamples<A>(int numSamples, Dist<A> dist)
        {
            var prior = dist.WeightedPrior();
            return Enumerable.Repeat(prior, numSamples).Sequence().Select(Samples);
        }


        /// <summary>
        /// Performs importance sampling on a distribution
        /// </summary>
        /// <returns>An importance sampled distribution</returns>
        public static Dist<A> ImportanceDist<A>(int numSamples, Dist<A> dist)
            => from probs in ImportanceSamples(numSamples, dist)
                from resampled in Categorical(probs)
                select resampled;

        /// <summary>
        /// Normalizes a list of samples
        /// </summary>
        public static Samples<A> Normalize<A>(Samples<A> samples)
        {
            var normConst = samples.SumProbs();
            return Samples(samples.Weights.Select(s => ItemProb(s.Item, s.Prob.Div(normConst))));
        }
    }
}

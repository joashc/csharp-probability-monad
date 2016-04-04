using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    /// <summary>
    /// Extension methods for importance sampling
    /// </summary>
    public static class ImportanceExt
    {
        /// <summary>
        /// Importance sample with given number of samples
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <param name="numSamples"></param>
        /// <returns></returns>
        public static Dist<Samples<A>> ImportanceSamples<A>(this Dist<A> dist, int numSamples)
        {
            return Importance.ImportanceSamples(numSamples, dist);
        }
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
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static Dist<Samples<A>> Resample<A>(Samples<A> samples)
        {
            var resamples = samples.Select(s => ItemProb(s, Prob(1)));
            var dist = Primitive(new FiniteDist<ItemProb<A>>(resamples).ToSampleDist());
            return Enumerable.Repeat(dist, samples.Weights.Count()).Sequence().Select(Samples);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static Samples<A> Flatten<A>(Samples<Samples<A>> samples)
        {
            return Samples(from outer in Normalize(samples).Weights
                           from inner in outer.Item.Weights
                           select ItemProb(inner.Item, inner.Prob.Mult(outer.Prob)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="n"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<Samples<A>> ImportanceSamples<A>(int n, Dist<A> dist)
        {
            var prior = dist.Prior();
            return Enumerable.Repeat(prior, n).Sequence().Select(Samples);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="n"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<A> ImportanceDist<A>(int n, Dist<A> dist)
        {
            return from probs in ImportanceSamples(n, dist)
                   from resampled in Categorical(probs)
                   select resampled;
        }

        /// <summary>
        /// Normalize a list of samples
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static Samples<A> Normalize<A>(Samples<A> samples)
        {
            var normConst = samples.SumProbs();
            return Samples(samples.Weights.Select(s => ItemProb(s.Item, s.Prob.Div(normConst))));
        }
    }
}

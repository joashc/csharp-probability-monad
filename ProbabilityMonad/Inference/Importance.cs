using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    public static class Importance
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static Dist<IEnumerable<ItemProb<A>>> Resample<A>(IEnumerable<ItemProb<A>> samples)
        {
            var resamples = samples.Select(s => ItemProb(s, Prob(1)));
            var dist = Primitive(new FiniteDist<ItemProb<A>>(resamples).ToSampleDist());
            return Enumerable.Repeat(dist, samples.Count()).Sequence();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static IEnumerable<ItemProb<A>> Flatten<A>(IEnumerable<ItemProb<IEnumerable<ItemProb<A>>>> samples)
        {
            return from outer in samples
                   from inner in outer.Item
                   select ItemProb(inner.Item, inner.Prob.Mult(outer.Prob));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="n"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<IEnumerable<ItemProb<A>>> ImportanceSamples<A>(int n, Dist<A> dist)
        {
            var prior = dist.Prior();
            return Enumerable.Repeat(prior, n).Sequence();
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
                   from resampled in SamplesToDist(probs)
                   select resampled;
        }

        /// <summary>
        /// Normalize a list of samples
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static IEnumerable<ItemProb<A>> Normalize<A>(IEnumerable<ItemProb<A>> samples)
        {
            var normConst = samples.Select(s => s.Prob.Value).Sum();
            return samples.Select(s => ItemProb(s.Item, s.Prob.Mult(Prob(normConst))));
        }
    }
}

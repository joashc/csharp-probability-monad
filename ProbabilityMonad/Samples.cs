using System;
using System.Collections.Generic;
using System.Linq;
using static ProbabilityMonad.Base;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ProbabilityMonad
{

    /// <summary>
    /// Extension methods for samples
    /// </summary>
    public static class SamplesExt
    {
        public static Samples<B> Select<A, B>(this Samples<A> self, Func<ItemProb<A>, ItemProb<B>> f)
        {
            return Samples(self.Weights.Select(f));
        }

        public static Samples<B> MapSample<A, B>(this Samples<A> self, Func<A, B> f)
        {
            return Samples(self.Weights.Select(ip => ItemProb(f(ip.Item), ip.Prob)));
        }

        /// <summary>
        /// Sum all probabilities in samples
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Prob SumProbs<A>(this Samples<A> self)
        {
            return Prob(self.Weights.Select(ip => ip.Prob.Value).Sum());
        }

        public static Samples<A> Normalize<A>(this Samples<A> self)
        {
            return Importance.Normalize(self);
        }

        /// <summary>
        /// Unzip samples into items and weights
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static Tuple<IEnumerable<A>, IEnumerable<Prob>> Unzip<A>(this Samples<A> samples)
        {
            return new Tuple<IEnumerable<A>, IEnumerable<Prob>>(samples.Weights.Select(ip => ip.Item), samples.Weights.Select(ip => ip.Prob));
        }
    }

    /// <summary>
    /// Wrapper class for IEnumerable<ItemProb<A>>
    /// Working with the unwrapped type is pretty unwieldy, especially when nested.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Samples<A> : IEnumerable<ItemProb<A>>
    {
        public readonly IEnumerable<ItemProb<A>> Weights;
        public Samples(IEnumerable<ItemProb<A>> list)
        {
            Weights = list;
        }

        public IEnumerator<ItemProb<A>> GetEnumerator()
        {
            return Weights.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Weights.GetEnumerator();
        }
    }
}

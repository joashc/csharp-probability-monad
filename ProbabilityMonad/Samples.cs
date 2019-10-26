using System;
using System.Collections.Generic;
using System.Linq;
using static ProbCSharp.ProbBase;
using System.Collections;

namespace ProbCSharp
{

    /// <summary>
    /// Extension methods for samples
    /// </summary>
    public static class SamplesExt
    {
        public static Samples<B> Select<A, B>(this Samples<A> self, Func<ItemProb<A>, ItemProb<B>> f)
            => Samples(self.Weights.Select(f));

        public static Samples<B> MapSample<A, B>(this Samples<A> self, Func<A, B> f)
            => Samples(self.Weights.Select(ip => ItemProb(f(ip.Item), ip.Prob)));

        /// <summary>
        /// Sum all probabilities in samples
        /// </summary>
        public static Prob SumProbs<A>(this Samples<A> self)
            => Prob(self.Weights.Select(ip => ip.Prob.Value).Sum());

        public static Samples<A> Normalize<A>(this Samples<A> self)
            => Importance.Normalize(self);

        /// <summary>
        /// Unzip samples into items and weights
        /// </summary>
        public static Tuple<IEnumerable<A>, IEnumerable<Prob>> Unzip<A>(this Samples<A> samples)
            => new Tuple<IEnumerable<A>, IEnumerable<Prob>>(samples.Weights.Select(ip => ip.Item), samples.Weights.Select(ip => ip.Prob));
    }

    // This is a Wrapper class for IEnumerable<ItemProb<A>>
    // Working with the unwrapped type is pretty unwieldy, especially when nested.
    /// <summary>
    /// Represents a list of samples
    /// </summary>
    public class Samples<A> : IEnumerable<ItemProb<A>>
    {
        public readonly IEnumerable<ItemProb<A>> Weights;
        public Samples(IEnumerable<ItemProb<A>> list)
            => Weights = list;

        public IEnumerator<ItemProb<A>> GetEnumerator()
            => Weights.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Weights.GetEnumerator();
    }
}

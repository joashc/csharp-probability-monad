using System;
using static ProbCSharp.ProbBase;
using System.Collections.Generic;
using System.Linq;

namespace ProbCSharp
{
    public static class MetropolisHastings
    {
        /// <summary>
        /// The metropolis algorithm with prior as proposal dist.
        /// It's a really slow mixing chain if the prior is too different from the posterior.
        /// </summary>
        /// <returns>A distribution of Markov chains</returns>
        public static Dist<IEnumerable<A>> MHPrior<A>(Dist<A> dist, int n)
        {
            var initial = new List<ItemProb<A>> { dist.WeightedPrior().Sample() };
            var chain = Iterate(n, dist.WeightedPrior, initial);
            return chain.Select(ipList => ipList.Select(ip => ip.Item));
        }

        // Performs n iterations of MH
        private static Dist<IEnumerable<ItemProb<A>>> Iterate<A>(int n, Func<Dist<ItemProb<A>>> proposal, List<ItemProb<A>> chain)
        {
            if (n <= 0) return new Pure<IEnumerable<ItemProb<A>>>(chain);
            var chainState = chain.Last();
            var newChain = chain;
            while (n > 0)
            {
                var nextDist = from candidate in proposal()
                               from accept in Primitive(BernoulliF(Prob(Math.Min(1.0, candidate.Prob.Div(chainState.Prob).Value))).ToSampleDist())
                               select accept ? candidate : chainState;
                var next = nextDist.Sample();
                chainState = next;
                newChain.Add(next);
                n--;
            }
            return new Pure<IEnumerable<ItemProb<A>>>(newChain);
        }

        /// <summary>
        /// Returns the value of the metropolis chain at specified index
        /// </summary>
        public static Dist<A> MHIndex<A>(Dist<A> dist, int n, int index)
        {
            return MHPrior(dist, n).Select(list => list.ElementAt(index));
        }

    }
} 

using System;
using static ProbabilityMonad.Base;
using System.Collections.Generic;
using System.Linq;

namespace ProbabilityMonad
{
    public static class MetropolisHastings
    {
        /// <summary>
        /// The metropolis algorithm with prior as proposal dist.
        /// This isn't that useful on its own, it's a really slow mixing chain
        /// if the prior is too different from the posterior.
        /// It's more to show off the SMC composition!
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Dist<IEnumerable<A>> MHPrior<A>(Dist<A> dist, int n)
        {
            var initial = new List<ItemProb<A>> { dist.Prior().Sample() };
            var chain = Iterate(n, () => dist.Prior(), initial);
            return chain.Select(ipList => ipList.Select(ip => ip.Item));
        }

        /// <summary>
        /// Performs n iterations of MH
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="n"></param>
        /// <param name="proposal"></param>
        /// <param name="chain"></param>
        /// <returns></returns>
        private static Dist<IEnumerable<ItemProb<A>>> Iterate<A>(int n, Func<Dist<ItemProb<A>>> proposal, List<ItemProb<A>> chain)
        {
            if (n <= 0) return new Pure<IEnumerable<ItemProb<A>>>(chain);
            var chainState = chain.Last();
            var newChain = chain;
            while (n > 0)
            {
                var nextDist = from candidate in proposal()
                               from accept in Primitive(Bernoulli(Prob(Math.Min(1.0, candidate.Prob.Div(chainState.Prob).Value))).ToSampleDist())
                               select accept ? candidate : chainState;
                var next = nextDist.Sample();
                chainState = next;
                newChain.Add(next);
                n--;
            }
            return new Pure<IEnumerable<ItemProb<A>>>(newChain);
        }

        public static Dist<A> MHIndex<A>(Dist<A> dist, int n, int index)
        {
            return MHPrior(dist, n).Select(list => list.ElementAt(index));
        }

    }
} 

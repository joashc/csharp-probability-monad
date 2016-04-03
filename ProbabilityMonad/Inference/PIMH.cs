using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityMonad
{
    public static class PIMH
    {
        public static Dist<IEnumerable<IEnumerable<ItemProb<A>>>> Run<A>(int n, int chainLen, Dist<A> dist)
        {
            return MetropolisHastings.MHPrior(dist.Run(new SMC<A>(n)), chainLen);
        }

        public static Dist<IEnumerable<ItemProb<A>>> RunIndexed<A>(int n, int chainIndex, Dist<A> dist)
        {
            return MetropolisHastings.MHIndex(dist.Run(new SMC<A>(n)), chainIndex, chainIndex);
        }
    }
}

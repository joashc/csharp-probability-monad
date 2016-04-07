using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityMonad
{
    public static class Pimh
    {
        public static Dist<IEnumerable<Samples<A>>> Run<A>(int n, int chainLen, Dist<A> dist)
        {
            return MetropolisHastings.MHPrior(dist.Run(new Smc<A>(n)), chainLen);
        }
    }
}

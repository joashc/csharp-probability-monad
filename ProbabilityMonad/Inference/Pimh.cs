using System.Collections.Generic;

namespace ProbCSharp
{
    public static class Pimh
    {
        /// <summary>
        /// Particle indepedent Metropolis-Hastings
        /// </summary>
        public static Dist<IEnumerable<Samples<A>>> Create<A>(int numParticles, int chainLen, Dist<A> dist)
        {
            return MetropolisHastings.MHPrior(dist.Run(new Smc<A>(numParticles)), chainLen);
        }
    }
}

using System;
using System.Linq;
using ProbCSharp;
using static ProbCSharp.ProbBase;

namespace ProbCSharp
{
    public static class KullbackLeibner
    {
        /// <summary>
        /// Calculates KL divergence of two finite distributions
        /// </summary>
        /// <param name="keyFunc">Groups identical samples</param>
        public static double KLDivergenceF<A, Key>(FiniteDist<A> distQ, FiniteDist<A> distP, Func<A, Key> keyFunc) where A : IComparable<A>
        {
            var qWeights = Enumerate(distQ, keyFunc);
            Func<A, Prob> qDensity = x =>
            {
                var weight = qWeights.FirstOrDefault(ip => keyFunc(ip.Item).Equals(keyFunc(x)));
                if (weight == null) return Prob(0);
                return weight.Prob;
            };

            var pWeights = Enumerate(distP, keyFunc);

            var divergences = pWeights.Weights
                                      .Select(w => w.Prob.Value * Math.Log(w.Prob.Div(qDensity(w.Item)).Value));
            return divergences.Sum();
        }

        /// <summary>
        /// Calculates KL divergence from list of samples and a finite distribution
        /// </summary>
        /// <param name="keyFunc">Groups identical samples</param>
        public static double KLDivergence<A, Key>(Samples<A> samples, FiniteDist<A> dist, Func<A, Key> keyFunc) where A : IComparable<A>
        {
            var sampleDist = CategoricalF(samples);
            return KLDivergenceF(sampleDist, dist, keyFunc);
        }
    }
}

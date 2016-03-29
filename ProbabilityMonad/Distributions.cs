using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;
using static ProbabilityMonad.ProbabilityFunctions;
using CSharpProbabilityMonad;
using MathNet.Numerics.Distributions;

namespace ProbabilityMonad
{
    public static class Distributions
    {
        /// <summary>
        /// Uniform distribution over list of items
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Dist<A> EnumUniformD<A>(IEnumerable<A> items)
        {
            var uniform = items.Select(i => new ItemProb<A>(i, Prob(1)));
            return new Dist<A>(Normalize(uniform));
        }

        /// <summary>
        /// Uniform distribution, using parameters
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Dist<A> UniformD<A>(params A[] items)
        {
            var itemList = new List<A>(items);
            return EnumUniformD(itemList);
        }

        /// <summary>
        /// Bernoulli distribution constructed from success probability
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static Dist<bool> Bernoulli(Prob prob)
        {
            return new Dist<bool>(ItemProb(true, prob), ItemProb(false, Prob(1 - prob.Value)));
        }

        /// <summary>
        /// Conditional distribution
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="cond"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<A> Conditional<A>(Func<A, Prob> cond, Dist<A> dist) 
        {
            var condDist = dist.Condition(cond).Distribution;
            return new Dist<A>(Normalize(condDist));
        }

        /// <summary>
        /// Normal distribution
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="var"></param>
        /// <returns></returns>
        public static SampleDist<double> Normal(double mean, double var)
        {
            return new SampleDist<double>(() => 
                MathNet.Numerics.Distributions.Normal.WithMeanVariance(mean, var).Sample());
        }

        /// <summary>
        /// Beta distribution
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static SampleDist<double> Beta(double alpha, double beta)
        {
            return new SampleDist<double>(() => new Beta(alpha, beta).Sample());
        }



    }
}

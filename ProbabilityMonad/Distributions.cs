using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;
using static ProbabilityMonad.ProbabilityFunctions;
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
        public static FiniteDist<A> EnumUniformD<A>(IEnumerable<A> items)
        {
            var uniform = items.Select(i => new ItemProb<A>(i, Prob(1)));
            return new FiniteDist<A>(Normalize(uniform));
        }

        /// <summary>
        /// Uniform distribution, using parameters
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static FiniteDist<A> UniformD<A>(params A[] items)
        {
            var itemList = new List<A>(items);
            return EnumUniformD(itemList);
        }

        /// <summary>
        /// Bernoulli distribution constructed from success probability
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static FiniteDist<bool> Bernoulli(Prob prob)
        {
            return new FiniteDist<bool>(ItemProb(true, prob), ItemProb(false, Prob(1 - prob.Value)));
        }

        /// <summary>
        /// Conditional distribution
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="cond"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static FiniteDist<A> SoftConditional<A>(Func<A, Prob> cond, FiniteDist<A> dist) 
        {
            var condDist = dist.ConditionSoft(cond).Distribution;
            return new FiniteDist<A>(Normalize(condDist));
        }

        public static FiniteDist<A> HardConditional<A>(Func<A, bool> cond, A item)
        {
            return new FiniteDist<A>(ItemProb(item, Prob(cond(item) ? 1 : 0)));
        }

        /// <summary>
        /// Normal distribution
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="var"></param>
        /// <returns></returns>
        public static ContDist<double> Normal(double mean, double var)
        {
            return new ContDist<double>(() => 
                MathNet.Numerics.Distributions.Normal.WithMeanVariance(mean, var).Sample());
        }

        public static Prob NormalPdf(double mean, double var, double x)
        {
            return Prob(MathNet.Numerics.Distributions.Normal.PDF(mean, var, x));
        }

        /// <summary>
        /// Beta distribution
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static ContDist<double> Beta(double alpha, double beta)
        {
            return new ContDist<double>(() => new Beta(alpha, beta).Sample());
        }

        public interface Dist<A> { }

        public class Pure<A> : Dist<A>
        {
            public readonly A Value;
            public Pure(A value)
            {
                Value = value;
            }
        }

        public class Primitive<A> : Dist<A>
        {
            public readonly ContDist<A> dist;
            public Primitive(ContDist<A> dist)
            {
                this.dist = dist;
            }
        }

        public class ConditionalC<A> : Dist<A>
        {
            public readonly Func<A, Prob> likelihood;
            public readonly Dist<A> dist;
            public ConditionalC(Func<A, Prob> likelihood, Dist<A> dist)
            {
                this.likelihood = likelihood;
                this.dist = dist;
            }
        }

    }
}

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
        public static FiniteDist<A> Conditional<A>(Func<A, Prob> cond, FiniteDist<A> dist) 
        {
            var condDist = dist.Condition(cond).Distribution;
            return new FiniteDist<A>(Normalize(condDist));
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


        public interface Inference<A>
        {
            Func<A, ContDist<A>> Return();
            Func<ContDist<B>, Func<B, ContDist<A>, ContDist<A>>> Bind<B>();
            Func<ContDist<A>, ContDist<A>> Primitive();
            Func<Func<A, Prob>, ConditionalDist<A>, ContDist<A>> Conditional();
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

        public class Bind<A,B> : Dist<A>
        {
            public readonly Dist<A> dist;
            public readonly Func<A, Dist<B>> bind;
            public Bind(Dist<A> dist, Func<A, Dist<B>> bind)
            {
                this.dist = dist;
                this.bind = bind;
            }
        }

        public class Primitive<A> : Dist<A>
        {
            public readonly Sampleable<A> dist;
            public Primitive(Sampleable<A> dist)
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

        public interface Sampleable<A> : Dist<A>
        {
            A Sample(Dist<A> dist);
        }





    }
}

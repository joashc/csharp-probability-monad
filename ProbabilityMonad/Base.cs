using System.Collections.Generic;
using System.Linq;
using ProbabilityMonad;
using System;

namespace ProbabilityMonad
{
    /// <summary>
    /// This class exports a whole bunch of constructors.
    /// Basically avoids us typing new X<A, B, C>()
    /// which can make code unreadable when we have really large types.
    /// </summary>
    public static class Base
    {
        // Singleton instance of the random generator to avoid repeated values in tight loops
        public static Random Gen = new Random();

        #region Primitive object constructors
        /// <summary>
        /// Probability constructor
        /// </summary>
        /// <param name="probability"></param>
        /// <returns></returns>
        public static Prob Prob(double probability)
        {
            return new LogProb(Math.Log(probability));
        }

        /// <summary>
        /// ItemProb constructor 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="item"></param>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static ItemProb<A> ItemProb<A>(A item, Prob prob)
        {
            return new ItemProb<A>(item, prob);
        }

        /// <summary>
        /// Samples constructor
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="itemProbs"></param>
        /// <returns></returns>
        public static Samples<A> Samples<A>(IEnumerable<ItemProb<A>> itemProbs)
        {
            return new Samples<A>(itemProbs);
        }

        /// <summary>
        /// Tuple constructor
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Tuple<A, B> Tuple<A, B>(A a, B b)
        {
            return new Tuple<A, B>(a, b);
        }
        #endregion

        #region Distribution constructors
        /// <summary>
        /// Finite uniform distribution over list of items
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static FiniteDist<A> EnumUniformF<A>(IEnumerable<A> items)
        {
            var uniform = Samples(items.Select(i => new ItemProb<A>(i, Prob(1))));
            return new FiniteDist<A>(Importance.Normalize(uniform));
        }

        /// <summary>
        /// Uniform distribution, using parameters
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Dist<A> UniformFromList<A>(IEnumerable<A> items)
        {
            return Primitive(EnumUniformF(items));
        }

        /// <summary>
        /// Finite uniform distribution, using parameters
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static FiniteDist<A> UniformF<A>(params A[] items)
        {
            var itemList = new List<A>(items);
            return EnumUniformF(itemList);
        }

        /// <summary>
        /// Uniform distribution, using parameters
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Dist<A> Uniform<A>(params A[] items)
        {
            return Primitive(UniformF(items));
        }

        /// <summary>
        /// Bernoulli distribution constructed from success probability
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static FiniteDist<bool> BernoulliF(Prob prob)
        {
            return new FiniteDist<bool>(ItemProb(true, prob), ItemProb(false, Prob(1 - prob.Value)));
        }

        /// <summary>
        /// Bernoulli distribution constructed from success probability
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static Dist<bool> Bernoulli(Prob prob)
        {
            return Primitive(BernoulliF(prob));
        }

        /// <summary>
        /// Normal distribution constructor
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="variance"></param>
        /// <returns></returns>
        public static NormalC NormalC(double mean, double variance)
        {
            return new NormalC(mean, variance, Gen);
        }

        public static Dist<double> Normal(double mean, double variance)
        {
            return Primitive(NormalC(mean, variance));
        }

        /// <summary>
        /// Beta distribution
        /// </summary>
        public static BetaC BetaC(double alpha, double beta)
        {
            return new BetaC(beta, alpha, Gen);
        }

        /// <summary>
        /// Beta distribution
        /// </summary>
        public static Dist<double> Beta(double alpha, double beta)
        {
            return Primitive(BetaC(alpha, beta));
        }

        public static FiniteDist<A> CategoricalF<A>(Samples<A> samples)
        {
            return new FiniteDist<A>(samples);
        }

        /// <summary>
        /// Lift a list of samples into the GADT dist type
        /// </summary> 
        /// <typeparam name="A"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static Dist<A> Categorical<A>(Samples<A> samples)
        {
            return Primitive(CategoricalF(samples).ToSampleDist());
        }
        #endregion

        #region GADT constructors

        /// <summary>
        /// Primitive constructor
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<A> Primitive<A>(ContDist<A> dist)
        {
            return new Primitive<A>(dist);
        }

        /// <summary>
        /// Primitive constructor for finite dists
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<A> Primitive<A>(FiniteDist<A> dist)
        {
            return new Primitive<A>(dist.ToSampleDist());
        }

        /// <summary>
        /// Pure constructor, monadic return
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dist<A> Return<A>(A value)
        {
            return new Pure<A>(value);
        }

        /// <summary>
        /// Conditional constructor
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="likelihood"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<A> Condition<A>(Func<A, Prob> likelihood, Dist<A> dist)
        {
            return new Conditional<A>(likelihood, dist);
        }

        #endregion

        #region Utility functions
        /// <summary>
        /// Aggregates probabilities of samples with identical values
        /// Uses a key function to find identical samples
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="Key"></typeparam>
        /// <param name="samples"></param>
        /// <param name="keyFunc"></param>
        /// <returns></returns>
        public static Samples<A> Compact<A, Key>(Samples<A> samples, Func<A, Key> keyFunc) where A : IComparable<A>
        {
            return Samples(CompactUnordered(samples, keyFunc).Weights.OrderBy(w => w.Item));
        }

        /// <summary>
        /// Aggregates probabilities of samples with identical values
        /// Uses a key function to find identical samples
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="Key"></typeparam>
        /// <param name="samples"></param>
        /// <param name="keyFunc"></param>
        /// <returns></returns>
        public static Samples<A> CompactUnordered<A, Key>(Samples<A> samples, Func<A, Key> keyFunc)
        {
            var compacted =
                samples.Weights
                    .GroupBy(ip => keyFunc(ip.Item))
                    .Select(g => 
                        ItemProb(
                            g.First().Item,
                            Prob(g.Select(ip => ip.Prob.Value).Sum())
                        )
                    );
            return Samples(compacted);
        }

        /// <summary>
        /// Aggregate & normalize weights, sorting in ascending order of values
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="Key"></typeparam>
        /// <param name="dist"></param>
        /// <param name="keyFunc"></param>
        /// <returns></returns>
        public static Samples<A> Enumerate<A, Key>(FiniteDist<A> dist, Func<A, Key> keyFunc) where A : IComparable<A>
        {
            return Importance.Normalize(Compact(dist.Explicit, keyFunc));
        }

        /// <summary>
        /// The probability density function for a given ContDist and point
        /// </summary>
        public static Prob Pdf(ContDist<double> dist, double y)
        {
            if (dist is NormalC)
            {
                var normal = dist as NormalC;
                return Prob(MathNet.Numerics.Distributions.Normal.PDF(normal.Mean, Math.Sqrt(normal.Variance), y));
            }
            if (dist is BetaC)
            {
                var beta = dist as BetaC;
                return Prob(MathNet.Numerics.Distributions.Beta.PDF(beta.alpha, beta.beta, y));
            }
            throw new NotImplementedException("No PDF for this distribution implemented");
        }

        /// <summary>
        /// The native IEnumerable<A>.Add is mutative
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<A> Append<A>(IEnumerable<A> list, A value)
        {
            var appendList = new List<A>(list);
            appendList.Add(value);
            return appendList;
        }
        #endregion
    }
}

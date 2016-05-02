using System.Collections.Generic;
using System.Linq;
using ProbCSharp;
using System;

namespace ProbCSharp
{
    /// <summary>
    /// This class exports a whole bunch of constructors.
    /// Basically avoids us typing new X<A, B, C>()
    /// which can make code unreadable when we have really large types.
    /// </summary>
    public static class ProbBase
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
        /// Bernoulli distribution constructed from success probability
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static Dist<bool> Bernoulli(double prob)
        {
            return Primitive(BernoulliF(Prob(prob)));
        }

        /// <summary>
        /// Bernoulli distribution constructed from two items, and probability of first item
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static Dist<A> Bernoulli<A>(double prob, A option1, A option2)
        {
            return Primitive(BernoulliF(Prob(prob))).Select(b => b ? option1 : option2);
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

        #region Parallel constructors
        /// <summary>
        /// Wraps the distribution to defer evaluation until explicitly parallelized
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<Dist<A>> Independent<A>(Dist<A> dist)
        {
            return new Independent<A>(dist);
        }

        /// <summary>
        /// Evaluates two distributions in parallel
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist1"></param>
        /// <param name="dist2"></param>
        /// <param name="run"></param>
        /// <returns></returns>
        public static Dist<A> RunIndependentWith<T1, T2, A>(Dist<T1> dist1, Dist<T2> dist2, Func<T1, T2, Dist<A>> run)
        {
            return new RunIndependent<T1, T2, A>(dist1, dist2, run);
        }

        /// <summary>
        /// Evaluates two distributions in parallel. The results are collected into a tuple.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dist1"></param>
        /// <param name="dist2"></param>
        /// <returns></returns>
        public static Dist<Tuple<T1, T2>> RunIndependent<T1, T2>(Dist<T1> dist1, Dist<T2> dist2)
        {
            return new RunIndependent<T1, T2, Tuple<T1, T2>>(dist1, dist2, (t1, t2) => Return(new Tuple<T1, T2>(t1, t2)));
        }

        /// <summary>
        /// Evaluates three distributions in parallel. The results are collected into a tuple.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="dist1"></param>
        /// <param name="dist2"></param>
        /// <param name="dist3"></param>
        /// <returns></returns>
        public static Dist<Tuple<T1, T2, T3>> RunIndependent<T1, T2, T3>(Dist<T1> dist1, Dist<T2> dist2, Dist<T3> dist3)
        {
            return new RunIndependent3<T1, T2, T3, Tuple<T1, T2, T3>>(dist1, dist2, dist3, (t1, t2, t3) => Return(new Tuple<T1, T2, T3>(t1, t2, t3)));
        }

        /// <summary>
        /// Evaluates three distributions in parallel
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist1"></param>
        /// <param name="dist2"></param>
        /// <param name="dist3"></param>
        /// <param name="run"></param>
        /// <returns></returns>
        public static Dist<A> RunIndependentWith<T1, T2, T3, A>(Dist<T1> dist1, Dist<T2> dist2, Dist<T3> dist3, Func<T1, T2, T3, Dist<A>> run)
        {
            return new RunIndependent3<T1, T2, T3, A>(dist1, dist2, dist3, run);
        }
        #endregion

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
        /// Pure constructor, monadic return
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dist<A> Dirac<A>(A value)
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

        /// <summary>
        /// Conditional constructor
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="likelihood"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Dist<A> Condition<A>(this Dist<A> dist, Func<A, Prob> likelihood)
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

        /// <summary>
        /// Sigmoid function
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
        #endregion
    }
}

using System.Collections.Generic;
using System.Linq;
using ProbCSharp;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

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
        public static Prob Prob(double probability)
        {
            return new LogProb(Math.Log(probability));
        }

        /// <summary>
        /// ItemProb constructor 
        /// </summary>
        public static ItemProb<A> ItemProb<A>(A item, Prob prob)
        {
            return new ItemProb<A>(item, prob);
        }

        /// <summary>
        /// Samples constructor
        /// </summary>
        public static Samples<A> Samples<A>(IEnumerable<ItemProb<A>> itemProbs)
        {
            return new Samples<A>(itemProbs);
        }

        /// <summary>
        /// Tuple constructor
        /// </summary>
        public static Tuple<A, B> Tuple<A, B>(A a, B b)
        {
            return new Tuple<A, B>(a, b);
        }
        #endregion

        #region Distribution constructors
        /// <summary>
        /// Finite uniform distribution over list of items. 
        /// Only composable with other finite distributions.
        /// </summary>
        public static FiniteDist<A> EnumUniformF<A>(IEnumerable<A> items)
        {
            var uniform = Samples(items.Select(i => new ItemProb<A>(i, Prob(1))));
            return new FiniteDist<A>(Importance.Normalize(uniform));
        }

        /// <summary>
        /// Uniform distribution over list of items
        /// </summary>
        public static Dist<A> UniformFromList<A>(IEnumerable<A> items)
        {
            return Primitive(EnumUniformF(items));
        }

        /// <summary>
        /// Uniform distribution over parameter items
        /// Only composable with other finite distributions.
        /// </summary>
        public static FiniteDist<A> UniformF<A>(params A[] items)
        {
            var itemList = new List<A>(items);
            return EnumUniformF(itemList);
        }

        /// <summary>
        /// Uniform distribution over parameter items
        /// </summary>
        public static Dist<A> Uniform<A>(params A[] items)
        {
            return Primitive(UniformF(items));
        }

        /// <summary>
        /// Bernoulli distribution constructed from success probability
        /// Only composable with other finite distributions.
        /// </summary>
        public static FiniteDist<bool> BernoulliF(Prob prob)
        {
            return new FiniteDist<bool>(ItemProb(true, prob), ItemProb(false, Prob(1 - prob.Value)));
        }

        /// <summary>
        /// Bernoulli distribution constructed from success probability
        /// </summary>
        public static Dist<bool> Bernoulli(Prob prob)
        {
            return Primitive(BernoulliF(prob));
        }

        /// <summary>
        /// Bernoulli distribution constructed from success probability
        /// </summary>
        public static Dist<bool> Bernoulli(double prob)
        {
            return Primitive(BernoulliF(Prob(prob)));
        }

        /// <summary>
        /// Bernoulli distribution constructed from two items, and probability of first item
        /// </summary>
        public static Dist<A> Bernoulli<A>(double prob, A option1, A option2)
        {
            return Primitive(BernoulliF(Prob(prob))).Select(b => b ? option1 : option2);
        }

        /// <summary>
        /// Categorical distribution
        /// Only composable with other finite distributions
        /// </summary>
        public static FiniteDist<A> CategoricalF<A>(Samples<A> samples)
        {
            return new FiniteDist<A>(samples);
        }

        /// <summary>
        /// Categorical distribution
        /// </summary> 
        public static Dist<A> Categorical<A>(Samples<A> samples)
        {
            return Primitive(CategoricalF(samples).ToSampleDist());
        }

        /// <summary>
        /// Primitive Normal distribution
        /// Only composable with other primitive distributions
        /// </summary>
        public static NormalPrimitive NormalPrimitive(double mean, double variance)
        {
            return new NormalPrimitive(mean, variance, Gen);
        }

        /// <summary>
        /// Primitive Normal distribution
        /// Only composable with other primitive distributions
        /// </summary>
        public static LogNormalPrimitive LogNormalPrimitive(double mean, double variance) {
            return new LogNormalPrimitive(mean, variance, Gen);
        }

      public static LogNormalPrimitive LogNormalPrimitiveMu(double mu, double sigma)
      {
         return new LogNormalPrimitive(mu, sigma,true, Gen);
      }

      /// <summary>
      /// Normal distribution
      /// </summary>
      public static Dist<double> Normal(double mean, double variance)
        {
            return Primitive(NormalPrimitive(mean, variance));
        }

        /// <summary>
        /// Log Normal distribution
        /// </summary>
        public static Dist<double> LogNormal(double mean, double variance) {
            return Primitive(LogNormalPrimitive(mean, variance));
        }

       public static Dist<double> LogNormalMu(double mu, double sigma)
       {
           return Primitive(LogNormalPrimitiveMu(mu, sigma));
       }


      /// <summary>
      /// Primitive Beta distribution
      /// Only composable with other primitive distributions
      /// </summary>
      public static BetaPrimitive BetaPrimitive(double alpha, double beta)
        {
            return new BetaPrimitive(alpha, beta, Gen);
        }

      /// <summary>
      /// Primitive Beta distribution
      /// Only composable with other primitive distributions
      /// </summary>
      public static GammaPrimitive GammaPrimitive(double shape, double rate) {
         return new GammaPrimitive(shape, rate, Gen);
      }

      public static MultiVariateNormalPrimitive MultiVariateNormalPrimitive(double[] mean, Matrix<double> covariance)
      {
         return new MultiVariateNormalPrimitive(mean, covariance, Gen);
      }

      /// <summary>
      /// Primitive Beta distribution
      /// Only composable with other primitive distributions
      /// </summary>
      public static DirichletPrimitive DirichletPrimitive(double[] alpha)
      {
         return new DirichletPrimitive(alpha, Gen);
      }
      /// <summary>
      /// Beta distribution
      /// </summary>
      public static Dist<double> Gamma(double shape, double rate) {
         return Primitive(GammaPrimitive(shape, rate));
      }

      /// <summary>
      /// Beta distribution
      /// </summary>
      public static Dist<double> Beta(double alpha, double beta)
      {
         return Primitive(BetaPrimitive(alpha, beta));
      }

      /// <summary>
      /// Beta distribution
      /// </summary>
      public static Dist<double[]> Dirichlet(double[] alpha)
      {
         return Primitive(DirichletPrimitive(alpha));
      }

      public static Dist<double[]> MultiVariateNormal(double[] mean, Matrix<double> covariance)
      {
         return Primitive(MultiVariateNormalPrimitive(mean, covariance));
      }
      #endregion

      #region GADT constructors

      #region Parallel constructors
      /// <summary>
      /// Wraps the distribution to defer evaluation until explicitly parallelized
      /// </summary>
      public static Dist<Dist<A>> Independent<A>(Dist<A> dist)
        {
            return new Independent<A>(dist);
        }

        /// <summary>
        /// Evaluates two distributions in parallel, passing the results to a function
        /// </summary>
        public static Dist<A> RunIndependentWith<T1, T2, A>(Dist<T1> dist1, Dist<T2> dist2, Func<T1, T2, Dist<A>> run)
        {
            return new RunIndependent<T1, T2, A>(dist1, dist2, run);
        }

        /// <summary>
        /// Evaluates two distributions in parallel. The results are collected into a tuple.
        /// </summary>
        public static Dist<Tuple<T1, T2>> RunIndependent<T1, T2>(Dist<T1> dist1, Dist<T2> dist2)
        {
            return new RunIndependent<T1, T2, Tuple<T1, T2>>(dist1, dist2, (t1, t2) => Return(new Tuple<T1, T2>(t1, t2)));
        }

        /// <summary>
        /// Evaluates three distributions in parallel. The results are collected into a tuple.
        /// </summary>
        public static Dist<Tuple<T1, T2, T3>> RunIndependent<T1, T2, T3>(Dist<T1> dist1, Dist<T2> dist2, Dist<T3> dist3)
        {
            return new RunIndependent3<T1, T2, T3, Tuple<T1, T2, T3>>(dist1, dist2, dist3, (t1, t2, t3) => Return(new Tuple<T1, T2, T3>(t1, t2, t3)));
        }

        /// <summary>
        /// Evaluates three distributions in parallel, passing the results to a function
        /// </summary>
        public static Dist<A> RunIndependentWith<T1, T2, T3, A>(Dist<T1> dist1, Dist<T2> dist2, Dist<T3> dist3, Func<T1, T2, T3, Dist<A>> run)
        {
            return new RunIndependent3<T1, T2, T3, A>(dist1, dist2, dist3, run);
        }
        #endregion

        /// <summary>
        /// Primitive constructor for continuous dists
        /// </summary>
        public static Dist<A> Primitive<A>(PrimitiveDist<A> dist)
        {
            return new Primitive<A>(dist);
        }

        /// <summary>
        /// Primitive constructor for finite dists
        /// </summary>
        public static Dist<A> Primitive<A>(FiniteDist<A> dist)
        {
            return new Primitive<A>(dist.ToSampleDist());
        }

        /// <summary>
        /// Pure constructor, monadic return
        /// </summary>
        public static Dist<A> Return<A>(A value)
        {
            return new Pure<A>(value);
        }

        /// <summary>
        /// Create a conditional distribution with given likelihood function and distribution
        /// </summary>
        public static Dist<A> Condition<A>(Func<A, Prob> likelihood, Dist<A> dist)
        {
            return new Conditional<A>(likelihood, dist);
        }

        /// <summary>
        /// Conditional constructor
        /// </summary>
        public static Dist<A> Condition<A>(this Dist<A> dist, Func<A, Prob> likelihood)
        {
            return new Conditional<A>(likelihood, dist);
        }

        #endregion

        #region Utility functions
        /// <summary>
        /// Aggregates probabilities of samples with identical values
        /// The samples are arranged in ascending order
        /// </summary>
        /// <param name="keyFunc">Used to identify identical values</param>
        public static Samples<A> Compact<A, Key>(Samples<A> samples, Func<A, Key> keyFunc) where A : IComparable<A>
        {
            return Samples(CompactUnordered(samples, keyFunc).Weights.OrderBy(w => w.Item));
        }

        /// <summary>
        /// Aggregates probabilities of samples with identical values
        /// </summary>
        /// <param name="keyFunc">Used to identify identical values</param>
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
        /// Aggregate & normalize samples
        /// The samples are arranged in ascending order
        /// </summary>
        public static Samples<A> Enumerate<A, Key>(FiniteDist<A> dist, Func<A, Key> keyFunc) where A : IComparable<A>
        {
            return Importance.Normalize(Compact(dist.Explicit, keyFunc));
        }

        /// <summary>
        /// The probability density function for a primitive distribution and point.
        /// Throws NotImplementedException if no PDF is defined for given distribution.
        /// </summary>
        public static Prob Pdf(PrimitiveDist<double[]> dist, double[] x)
        {

            if (dist is DirichletPrimitive)
            {
                var dir = dist as DirichletPrimitive;
                var d = new MathNet.Numerics.Distributions.Dirichlet(dir.alpha);
                 
                return Prob(d.Density(x));
            }

            if (dist is MultiVariateNormalPrimitive)
            {
               var d = dist as MultiVariateNormalPrimitive;
               
               return Prob(d.mvn.Density(DenseMatrix.OfRowArrays(x)));
            }
         throw new NotImplementedException("No PDF for this distribution implemented");
        }

        /// <summary>
        /// The probability density function for a primitive distribution and point.
        /// Throws NotImplementedException if no PDF is defined for given distribution.
        /// </summary>
        public static Prob Pdf(PrimitiveDist<double> dist, double y)
        {
            if (dist is NormalPrimitive)
            {
                var normal = dist as NormalPrimitive;
                return Prob(MathNet.Numerics.Distributions.Normal.PDF(normal.Mean, Math.Sqrt(normal.Variance), y));
            }
            if (dist is LogNormalPrimitive) {
                var lognormal = dist as LogNormalPrimitive;
                return Prob(MathNet.Numerics.Distributions.LogNormal.PDF(lognormal.mu, lognormal.sigma, y));
            }
            if (dist is BetaPrimitive)
            {
                var beta = dist as BetaPrimitive;
                return Prob(MathNet.Numerics.Distributions.Beta.PDF(beta.alpha, beta.beta, y));
            }
            if (dist is GammaPrimitive) {
                var gamma = dist as GammaPrimitive;
                return Prob(MathNet.Numerics.Distributions.Gamma.PDF(gamma.shape, gamma.rate, y));
            }
            throw new NotImplementedException("No PDF for this distribution implemented");
        }

        /// <summary>
        /// The probability mass function for a primitive distribution and point.
        /// Throws NotImplementedException if no PMF is defined for given distribution.
        /// </summary>
        public static Prob Pmf(PrimitiveDist<int> dist, int y)
        {
            if (dist is PoissonPrimitive)
            {
                var poisson = dist as PoissonPrimitive;
                return Prob(MathNet.Numerics.Distributions.Poisson.PMF(poisson.Lambda, y));
            }
            throw new NotImplementedException("No PDF for this distribution implemented");
        }

        /// <summary>
        /// The probability density function for a distribution and point.
        /// Throws ArgumentException if the distribution is not a Primitive.
        /// </summary>
        public static Prob Pdf(Dist<double> dist, double y)
        {
            if (dist is Primitive<double>)
            {
                var primitive = dist as Primitive<double>;
                return Pdf(primitive.dist, y);
            }
            throw new ArgumentException("Can only calculate PDF for primitive distributions");
        }

      public static Prob Pdf(Dist<double[]> dist, double[] y)
      {
         if (dist is Primitive<double[]>)
         {
            var primitive = dist as Primitive<double[]>;
            return Pdf(primitive.dist, y);
         }
         throw new ArgumentException("Can only calculate PDF for primitive distributions");
      }


      /// <summary>
      /// Appends a value to a list. Non-mutative.
      /// </summary>
      public static IEnumerable<A> Append<A>(IEnumerable<A> list, A value)
        {
            var appendList = new List<A>(list);
            appendList.Add(value);
            return appendList;
        }

        /// <summary>
        /// Sigmoid function
        /// </summary>
        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
        #endregion
    }
}

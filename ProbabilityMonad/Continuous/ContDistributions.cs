using System;
using static ProbabilityMonad.Base;
using System.Security.Cryptography;

namespace ProbabilityMonad
{
    /// <summary>
    /// Normal dist
    /// </summary>
    public class Normal : ContDist<double>
    {
        public double Mean { get; }
        public double Variance { get; }
        public Random Gen {get;}
        public Normal(double mean, double variance, Random gen)
        {
            Mean = mean;
            Variance = variance;
            Gen = gen;
        }

        public Func<double> Sample
        {
            get
            {
                return () => MathNet.Numerics.Distributions.Normal
                        .WithMeanVariance(Mean, Variance, Gen)
                        .Sample();
            }
        }
    }

    /// <summary>
    /// Beta dist
    /// </summary>
    public class Beta : ContDist<double>
    {
        public double alpha;
        public double beta;
        public MathNet.Numerics.Distributions.Beta dist;
        public Beta(double alpha, double beta, Random gen)
        {
            this.alpha = alpha;
            this.beta = beta;
            dist = new MathNet.Numerics.Distributions.Beta(alpha, beta, gen);
        }

        public Func<double> Sample
        {
            get { return () => dist.Sample(); }
        }
    }
}

using System;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    /// <summary>
    /// Normal dist
    /// </summary>
    public class Normal : ContDist<double>
    {
        public double Mean { get; }
        public double Variance { get; }
        public Normal(double mean, double variance)
        {
            Mean = mean;
            Variance = variance;
        }

        public Func<double> Sample
        {
            get
            {
                return () => MathNet.Numerics.Distributions.Normal
                        .WithMeanVariance(Mean, Variance)
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
        public Beta(double alpha, double beta)
        {
            this.alpha = alpha;
            this.beta = beta;
            dist = new MathNet.Numerics.Distributions.Beta(alpha, beta);
        }

        public Func<double> Sample
        {
            get { return () => dist.Sample(); }
        }
    }
}

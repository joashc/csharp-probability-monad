using System;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Normal distribution
    /// </summary>
    public class NormalPrimitive : PrimitiveDist<double>
    {
        public double Mean { get; }
        public double Variance { get; }
        public Random Gen {get;}
        public NormalPrimitive(double mean, double variance, Random gen)
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
}

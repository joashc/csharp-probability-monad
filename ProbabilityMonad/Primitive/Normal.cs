using MathNet.Numerics.Distributions;
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
        public Normal normal { get; }

      public NormalPrimitive(double mean, double variance, Random gen)
      {
            Mean = mean;
            Variance = variance;
            Gen = gen;
            normal = Normal.WithMeanVariance(Mean, Variance, Gen);
      }

       public Func<double> Sample
       {
            get
            {
                return () => normal.Sample();
            }
        }
    }
}

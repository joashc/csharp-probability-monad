using System;
using MathNet.Numerics.Distributions;

namespace ProbCSharp {
   /// <summary>
   /// Primitive Beta distribution
   /// </summary>
   public class LogNormalPrimitive : PrimitiveDist<double>
   {
      public double mean;
      public double variance;
      public double mu;
      public double sigma;
      public LogNormal dist;
      public LogNormalPrimitive(double mu, double sigma, bool dummy, Random gen)
      {
         dist = new LogNormal(mu, sigma);
         mu = dist.Mu;
         sigma = dist.Sigma;
      }
      public LogNormalPrimitive(double mean, double variance, Random gen)
      {
         this.mean = mean;
         this.variance = variance;
         dist = LogNormalCreate.fromMeanVariance(mean, variance); // MathNet.Numerics.Distributions.LogNormal(mu, sigma);
         mu = dist.Mu;
         sigma = dist.Sigma;
      }

      public Func<double> Sample
      {
         get { return () => dist.Sample(); }
      }
   }
}
 
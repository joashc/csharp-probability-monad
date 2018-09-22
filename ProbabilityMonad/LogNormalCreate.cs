using MathNet.Numerics.Distributions;

namespace ProbCSharp
{
   class LogNormalCreate
   {
      public static LogNormal fromMeanVariance(double mean, double variance)
      {
         return LogNormal.WithMeanVariance(mean, variance);
      }
   }
}

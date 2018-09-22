using System;
using MathNet.Numerics.Distributions;

namespace ProbCSharp
{
   public class ContinuousUniformPrimitive : PrimitiveDist<double>
   {
      public ContinuousUniform dist;
      public ContinuousUniformPrimitive(double lower, double upper, Random gen)
      { 
         dist = new ContinuousUniform(lower,upper);
      }
      public ContinuousUniformPrimitive(Random gen)
      {
         dist = new ContinuousUniform();
      }
      public Func<double> Sample
      {
         get { return () => dist.Sample(); }
      }
   }
}

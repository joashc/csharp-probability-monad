using System; 

namespace ProbCSharp
{
   public class ContinuousUniformPrimitive : PrimitiveDist<double>
   {
      public MathNet.Numerics.Distributions.ContinuousUniform dist;
      public ContinuousUniformPrimitive(double lower, double upper, Random gen)
      { 
         dist = new MathNet.Numerics.Distributions.ContinuousUniform(lower,upper);
      }
      public ContinuousUniformPrimitive(Random gen)
      {
         dist = new MathNet.Numerics.Distributions.ContinuousUniform();
      }
      public Func<double> Sample
      {
         get { return () => dist.Sample(); }
      }
   }
}

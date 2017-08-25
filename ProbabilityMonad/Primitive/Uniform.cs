using System;
using MathNet.Numerics.Distributions;

namespace ProbCSharp
{
    public class UniformPrimitive : PrimitiveDist<double>
    {
        public double Lower { get; }
        public double Upper { get; }
        private readonly ContinuousUniform dist;
        public UniformPrimitive()
        {
            
            dist = new ContinuousUniform();
            Lower = dist.LowerBound;
            Upper = dist.UpperBound;
        }
        public UniformPrimitive(double lower, double upper)
        {
            dist = new ContinuousUniform(lower, upper);
        }
        public Func<double> Sample => () => dist.Sample();
    }
}

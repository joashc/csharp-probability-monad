using System;
using MathNet.Numerics.Distributions;

namespace ProbCSharp
{
    public class ExponentialPrimitive : PrimitiveDist<double>
    {
        public double Rate { get; private set; }
        private Exponential dist;

        public ExponentialPrimitive(double rate)
        {
            Rate = rate;
            dist = new Exponential(rate);
        }
        public Func<double> Sample => () => dist.Sample();
    }
}

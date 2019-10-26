using System;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Beta distribution
    /// </summary>
    public class BetaPrimitive : PrimitiveDist<double>
    {
        public double alpha;
        public double beta;
        public MathNet.Numerics.Distributions.Beta dist;

        public BetaPrimitive(double alpha, double beta, Random gen)
        {
            this.alpha = alpha;
            this.beta = beta;
            dist = new MathNet.Numerics.Distributions.Beta(alpha, beta, gen);
        }

        public Func<double> Sample
            => () => dist.Sample();
    }
}

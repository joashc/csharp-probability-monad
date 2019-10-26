using System;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Dirichlet distribution
    /// </summary>
    public class DirichletPrimitive : PrimitiveDist<double[]>
    {
        public double[] alpha;

        public MathNet.Numerics.Distributions.Dirichlet dist;

        public DirichletPrimitive(double[] alpha, Random gen)
        {
            this.alpha = alpha;

            dist = new MathNet.Numerics.Distributions.Dirichlet(alpha, gen);
        }

        public Func<double[]> Sample
            => () => dist.Sample();
    }
}

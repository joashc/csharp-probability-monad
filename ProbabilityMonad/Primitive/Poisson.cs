using System;
using MathNet.Numerics.Distributions;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Poisson distribution
    /// If lambda is non-integer, note it is subject to a floor operation
    /// </summary>
    public class PoissonPrimitive : PrimitiveDist<int>
    {
        public double Lambda;
        public Poisson Dist;
        public Random Gen;

        public PoissonPrimitive(double lambda, Random gen)
        {
            Lambda = lambda;
            Dist = new Poisson(Math.Floor(lambda));
            Gen = gen;
        }

        public Func<int> Sample
            => () => Dist.Sample();
    }
}

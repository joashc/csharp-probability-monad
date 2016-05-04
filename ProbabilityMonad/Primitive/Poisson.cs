using System;
using MathNet.Numerics.Distributions;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Poisson distribution
    /// </summary>
    public class PoissonPrimitive : PrimitiveDist<int>
    {
        public int Lambda;
        public Poisson Dist;
        public Random Gen;
        public PoissonPrimitive(int lambda, Random gen)
        {
            Lambda = lambda;
            Dist = new Poisson(lambda);
            Gen = gen;
        }

        public Func<int> Sample
        {
            get
            {
                return () => Dist.Sample();
            }
        }
    }
}

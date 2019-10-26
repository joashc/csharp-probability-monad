using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Wishart distribution
    /// </summary>
    public class WishartPrimitive : PrimitiveDist<Matrix<double>>
    {
        public double DegreesOfFreedom { get; }
        public Matrix<double> Scale { get; }
        public Random Gen { get; }
        public Wishart Wishart { get; }

        public WishartPrimitive(double dof, Matrix<double> scale, Random gen)
        {
            DegreesOfFreedom = dof;
            Scale = scale;
            Gen = gen;
            Wishart = new Wishart(dof, scale);
        }

        public Func<Matrix<double>> Sample
            => () => Wishart.Sample();
    }
}

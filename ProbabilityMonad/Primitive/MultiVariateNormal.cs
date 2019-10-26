using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Multivariate Normal distribution
    /// </summary>
    public class MultiVariateNormalPrimitive : PrimitiveDist<double[]>
    {
        public double[] Mean { get; }
        public Matrix<double> Covariance { get; }
        public Random Gen { get; }
        public MatrixNormal mvn { get; }

        public static MatrixNormal CreateMultivariateNormal(double[] meanVector, Matrix<double> cv)
            => new MatrixNormal(
                m: DenseMatrix.OfRowArrays(meanVector),
                v: DenseMatrix.OfRowArrays(new double[][] { new double[] { 1.0 } }),
                k: cv);

        public MultiVariateNormalPrimitive(double[] mean, Matrix<double> covariance, Random gen)
        {
            Mean = mean;
            Covariance = covariance;
            Gen = gen;
            mvn = CreateMultivariateNormal(mean, covariance);
        }

        public Func<double[]> Sample
            => () => mvn.Sample().Row(0).ToArray();
    }
}

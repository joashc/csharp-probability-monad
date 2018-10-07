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

      static public MatrixNormal CreateMultivariateNormal(double[] meanVector, Matrix<double> cv)
      {
         return new MatrixNormal(DenseMatrix.OfRowArrays(meanVector), 
                                 DenseMatrix.OfRowArrays(new double[][] { new double[] { 1.0 } }), 
                                 cv);
      }

      public MultiVariateNormalPrimitive(double[] mean, Matrix<double> covariance, Random gen)
      {
         Mean = mean;
         Covariance = covariance;
         Gen = gen;
         mvn = MultiVariateNormalPrimitive.CreateMultivariateNormal(mean, covariance);
      }

      public Func<double[]> Sample
      {
         get
         {
            return () => mvn.Sample().Row(0).ToArray();
         }
      }
   }
}

 
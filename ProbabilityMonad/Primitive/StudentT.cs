using System;

namespace ProbCSharp
{
    public class StudentTPrimitive : PrimitiveDist<double>
    {
        public double Location { get; }
        public double Scale { get; }
        public double Normality { get; }
        public Random Gen { get; }
        public StudentTPrimitive(double location, double scale, double normality, Random gen)
        {
            Location = location;
            Scale = scale;
            Normality = normality;
            Gen = gen;
        }
        public Func<double> Sample => () => new MathNet.Numerics.Distributions.StudentT(Location, Scale, Normality, Gen).Sample();
    }
}

using System;
using MathNet.Numerics.Distributions;

namespace ProbCSharp
{
    public class StudentTPrimitive : PrimitiveDist<double>
    {
        public double Location { get; }
        public double Scale { get; }
        public double Normality { get; }
        public Random Gen { get; }
        private readonly StudentT dist;
        public StudentTPrimitive(double location, double scale, double normality, Random gen)
        {
            Location = location;
            Scale = scale;
            Normality = normality;
            Gen = gen;
            dist = new StudentT(Location, Scale, Normality, Gen);
        }
        public Func<double> Sample => () => dist.Sample();
    }
}

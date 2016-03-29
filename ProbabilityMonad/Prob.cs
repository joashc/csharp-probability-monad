using System;

namespace CSharpProbabilityMonad
{
    /// <summary>
    /// Interface for probability wrapper so we can easily switch to log likelihoods etc.
    /// </summary>
    public interface Prob : IEquatable<Prob>, IComparable<Prob>
    {
        double Value { get; }
        Prob Mult(Prob other);
    }

    /// <summary>
    /// Simple probability wrapper using double
    /// </summary>
    public class DoubleProb : Prob
    {
        public double Value { get; }
        public DoubleProb(double probability)
        {
            Value = probability;
        }

        public override string ToString()
        {
            var rounded = Math.Floor(Value * 1000) / 1000;
            return $"{rounded*100}%";
        }

        public Prob Mult(Prob other)
        {
            return new DoubleProb(Value * other.Value);
        }

        public int CompareTo(Prob other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(Prob other)
        {
            return Value.Equals(other.Value);
        }
    }

}

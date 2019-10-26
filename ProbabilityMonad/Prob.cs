using System;

namespace ProbCSharp
{
    /// <summary>
    /// Interface for probability wrapper so we can easily switch to log likelihoods
    /// </summary>
    public interface Prob : IEquatable<Prob>, IComparable<Prob>
    {
        double Value { get; }
        double LogValue { get; }
        Prob Mult(Prob other);
        Prob Div(Prob other);
    }

    /// <summary>
    /// Simple probability wrapper using double
    /// </summary>
    public class DoubleProb : Prob
    {
        public double Value { get; }

        public double LogValue
            => Math.Log(Value);

        public DoubleProb(double probability)
            => Value = probability;

        public override string ToString()
        {
            var rounded = Math.Floor(Value * 1000) / 1000;
            return $"{rounded*100}%";
        }

        public Prob Mult(Prob other)
            => new DoubleProb(Value * other.Value);

        public Prob Div(Prob other)
            => new DoubleProb(Value / other.Value);

        public int CompareTo(Prob other)
            => Value.CompareTo(other.Value);

        public bool Equals(Prob other)
            => Value.Equals(other.Value);
    }

    /// <summary>
    /// Probability wrapper using log probabilities
    /// </summary>
    public class LogProb : Prob
    {
        public double logProb;
        public LogProb(double logProb)
        {
            this.logProb = logProb;
            if (Double.IsNegativeInfinity(logProb)) {
                this.logProb = -1e300;
            }
        }

        public double Value
            => Math.Exp(logProb);

        public double LogValue
            => logProb;

        public override string ToString()
            => $"{Value*100:G3}%";

        public int CompareTo(Prob other)
            => logProb.CompareTo(other.LogValue);

        public Prob Div(Prob other)
            => new LogProb(logProb - other.LogValue);

        public bool Equals(Prob other)
            => other.LogValue == logProb;

        public Prob Mult(Prob other)
            => new LogProb(logProb + other.LogValue);
    }
}

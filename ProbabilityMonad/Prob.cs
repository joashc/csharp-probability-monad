using System;
using System.Diagnostics;

namespace ProbabilityMonad
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
        {
            get
            {
                return Math.Log(Value);
            }
        }

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

        public Prob Div(Prob other)
        {
            return new DoubleProb(Value / other.Value);
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
        {
            get
            {
                return Math.Exp(logProb);
            }
        }

        public double LogValue
        {
            get
            {
                return logProb;
            }
        }

        public override string ToString()
        {
            var str = $"{Value*100:G3}%";
            return str;
        }

        public int CompareTo(Prob other)
        {
            return logProb.CompareTo(other.LogValue);
        }

        public Prob Div(Prob other)
        {
            return new LogProb(logProb - other.LogValue);
        }

        public bool Equals(Prob other)
        {
            return other.LogValue == logProb;
        }

        public Prob Mult(Prob other)
        {
            return new LogProb(logProb + other.LogValue);
        }
    }
}

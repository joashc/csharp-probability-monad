using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbCSharp {
    /// <summary>
    /// Primitive Beta distribution
    /// </summary>
    public class GammaPrimitive : PrimitiveDist<double> {
        public double shape;
        public double rate;
        public MathNet.Numerics.Distributions.Gamma dist;
        public GammaPrimitive(double shape, double rate, Random gen) {
            this.shape = shape;
            this.rate = rate;
            dist = new MathNet.Numerics.Distributions.Gamma(shape, rate);
        }

        public Func<double> Sample {
            get { return () => dist.Sample(); }
        }
    }
}

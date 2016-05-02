using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad.Test.Models
{
    public static class IndianGpaModel
    {
        public static Dist<double>
        AmericanGpa =
            from minMax in Independent(Bernoulli(0.85, 1.0, 0.0))
            from dist in Independent(Beta(2, 8))
            from atMinMax in Bernoulli(0.05)
            from gpa in atMinMax ? minMax : dist
            select gpa * 4;

        public static Dist<double>
        IndianGpa =
            from minMax in Independent(Bernoulli(0.9, 1.0, 0.0))
            from dist in Independent(Beta(5, 5))
            from atMinMax in Bernoulli(0.1)
            from gpa in atMinMax ? minMax : dist
            select gpa * 10;
    }
}

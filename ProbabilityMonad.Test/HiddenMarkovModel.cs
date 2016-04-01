using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using ProbabilityMonad;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad.Test
{
    [TestClass]
    public class HiddenMarkovModel
    {
        [TestMethod]
        public void HiddenMarkov()
        {
            // Latent variables are discrete
            var states = new List<int> { -1, 0, 1 };

            Func<List<double>, FiniteDist<int>> 
            transitionDist = ps => new FiniteDist<int>(
                states.Zip(ps, (s, p) => new ItemProb<int>(s, Prob(p)))
            );

            var tNeg1 = transitionDist(new List<double> { 0.1,  0.4, 0.5  });
            var t0    = transitionDist(new List<double> { 0.2,  0.6, 0.2  });
            var t1    = transitionDist(new List<double> { 0.15, 0.7, 0.15 });

            Func<int, FiniteDist<int>> transitionMatrix = i =>
            {
                if (i == -1) return tNeg1;
                if (i == 0) return t0;
                if (i == 1) return t1;
                throw new ArgumentException("Invalid state specified");
            };
            
            var startDist = EnumUniformD(states.Select(s => new List<int> { s }));

            // Model the observed values as latent variables with gaussian noise
            Func<int, double, Prob>
            emission = (x, y) => Pdf(Normal(x, 1), y);

            Func<int, List<int>, List<int>>
            cons = (x,xs) => {
                var list = new List<int>();
                list.Add(x);
                list.AddRange(xs);
                return list;
            };

            Func<FiniteDist<List<int>>, double, FiniteDist<List<int>>>
            expand = (d, y) =>
                (from rest in d
                from x in transitionMatrix(rest[0])
                select cons(x, rest))
                .ConditionSoftUnnormalized(xs => emission(xs[0], y));

            List<double> observedValues = new List<double>
            {
                0.9,0.8,0.7,0,-0.025,5,2,0.1,0,0.13
            };

            var hmm = observedValues.Aggregate(startDist, expand, x => x);
        }

    }
}

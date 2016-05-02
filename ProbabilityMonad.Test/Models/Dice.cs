using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbCSharp.ProbBase;

namespace ProbCSharp.Test.Models
{
    public static class Dice
    {
        /// <summary>
        /// Exact distribution of sum of n independent die rolls
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static FiniteDist<int> DieExact(int n)
        {
            if (n == 0) return UniformF(0);
            var die = UniformF(1, 2, 3, 4, 5, 6);
            if (n == 1) return die;
            return from roll in die
                   from rest in new FiniteDist<int>(Importance.Normalize(Compact(DieExact(n - 1).Explicit, a => a)))
                   select roll + rest;
        }

        /// <summary>
        /// Exact distribution of sum of n independent die rolls, conditioned with likelihood inversely proportional to the sum
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static FiniteDist<int> ConditionalDieExact(int n)
        {
            return DieExact(n).ConditionSoftUnnormalized(
                x => {
                    return Prob(1.0 / Math.Pow(x, 2));
                });
        }

        /// <summary>
        /// Stochastic version of DieExact
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Dist<int> Die(int n)
        {
            return Primitive(DieExact(n));
        }

        /// <summary>
        /// Stochastic version of ConditionalDieExact
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Dist<int> ConditionalDie(int n)
        {
            return Condition(x => Prob(1.0/Math.Pow(x, 2)), Die(n));
        }
    }
}

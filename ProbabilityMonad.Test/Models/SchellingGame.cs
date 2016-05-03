using System;
using static ProbCSharp.ProbBase;

namespace ProbCSharp.Test.Models
{
    public static class SchellingGame
    {
        public static Func<Prob, FiniteDist<string>>
        location = pubPref => from visitPub in BernoulliF(pubPref)
                              select visitPub ? "pub" : "starbucks";

        public static void Meet()
        {
            var meetByChance = from amy in location(Prob(0.6))
                               from bob in location(Prob(0.6))
                               select new Tuple<string, string>(amy, bob);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad.Test.Models
{
    public static class ClinicalTrial
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

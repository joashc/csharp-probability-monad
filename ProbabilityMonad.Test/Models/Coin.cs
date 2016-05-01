using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbCSharp.ProbBase;

namespace ProbCSharp.Test.Models
{
    public static class CoinExt
    {
        public static Coin Heads = new Coin(true);

        public static Coin Tails = new Coin(false);

        /// <summary>
        /// Update weight distribution based on one coin flip
        /// </summary>
        public static Func<Coin, Dist<double>, Dist<double>>
        FlipUpdate = (coin, dist) => Condition(w => Prob(coin.Heads ? w : 1 - w), dist);

        /// <summary>
        /// Update weight distribution based on a series of coin flips
        /// </summary>
        public static Func<List<Coin>, Dist<double>, Dist<double>>
        FlipsUpdate = (coins, dist) => coins.Aggregate(dist, (d, t) => FlipUpdate(t, d));

        /// <summary>
        /// Update exact weight distribution based on one coin flip
        /// </summary>
        public static Func<Coin, FiniteDist<double>, FiniteDist<double>>
        FlipUpdateExact = (coin, dist) => dist.ConditionSoft(w => Prob(coin.Heads ? w : 1 - w));

        /// <summary>
        /// Update exact weight distribution based on a series of coin flips
        /// </summary>
        public static Func<IEnumerable<Coin>, FiniteDist<double>, FiniteDist<double>>
        FlipsUpdateExact = (coins, dist) => coins.Aggregate(dist, (d, t) => FlipUpdateExact(t, d));
    }

    public class Coin
    {
        public Coin(bool heads)
        {
            Heads = heads;
        }

        public bool Heads;
        public bool Tails { get { return !Heads; } }
    }
}

using System;
using static ProbabilityMonad.Base;
using System.Collections.Generic;
using System.Linq;

namespace ProbabilityMonad
{
    /// <summary>
    /// Useful extension methods for finite distributions.
    /// </summary>
    public static class FiniteExtensions
    {
        /// <summary>
        /// Pick a value from a distribution using a probability
        /// </summary>
        public static A Pick<A>(this FiniteDist<A> distribution, Prob pickProb)
        {
            var probVal = pickProb.Value;
            foreach (var prob in distribution.Explicit.Weights)
            {
                if (probVal < prob.Prob.Value)
                {
                    return prob.Item;
                }
                probVal -= prob.Prob.Value;
            }
            throw new ArgumentException("Sampling failed");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static ContDist<A> ToSampleDist<A>(this FiniteDist<A> dist)
        {
            return new ContDistImpl<A>(() =>
            {
                var rand = new MathNet.Numerics.Distributions.ContinuousUniform().Sample();
                return dist.Pick(Prob(rand));
            });
        }

        /// <summary>
        /// Returns the probability of a certain event
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="distribution"></param>
        /// <param name="eventTest"></param>
        /// <returns></returns>
        public static Prob ProbOf<A>(this FiniteDist<A> dist, Func<A, bool> eventTest)
        {
            var matches = dist.Explicit.Weights.Where(p => eventTest(p.Item));
            if (!matches.Any()) return Prob(0);
            return Prob(matches.Select(p => p.Prob.Value).Sum());
        }

        /// <summary>
        /// Reweight by a probability that depends on associated item
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="distribution"></param>
        /// <param name="likelihood"></param>
        /// <returns></returns>
        public static FiniteDist<A> ConditionSoft<A>(this FiniteDist<A> distribution, Func<A, Prob> likelihood)
        {
            return new FiniteDist<A>(
                distribution.Explicit
                    .Select(p => ItemProb(p.Item, likelihood(p.Item).Mult(p.Prob)))
                    .Normalize()
            );
        }

        /// <summary>
        /// Reweight by a probability that depends on associated item, without normalizing
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="distribution"></param>
        /// <param name="likelihood"></param>
        /// <returns></returns>
        public static FiniteDist<A> ConditionSoftUnnormalized<A>(this FiniteDist<A> distribution, Func<A, Prob> likelihood)
        {
            return new FiniteDist<A>(
                distribution.Explicit.Select(p => ItemProb(p.Item, likelihood(p.Item).Mult(p.Prob)))
            );
        }

        /// <summary>
        /// Hard reweight by a condition that depends on associated item
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="distribution"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static FiniteDist<A> ConditionHard<A>(this FiniteDist<A> distribution, Func<A, bool> condition)
        {
            return new FiniteDist<A>(
                distribution.Explicit
                    .Select(p => ItemProb(p.Item, condition(p.Item) ? p.Prob : Prob(0)))
                    .Normalize()
            );
                    
                
        }

        /// <summary>
        /// Normalize a finite dist
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static FiniteDist<A> Normalize<A>(this FiniteDist<A> dist)
        {
            return new FiniteDist<A>(dist.Explicit.Normalize());
        }
        
        /// <summary>
        /// Join two independent distributions
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static FiniteDist<Tuple<A,B>> Join<A, B>(this FiniteDist<A> self, FiniteDist<B> other)
        {
            return from a in self
                   from b in other
                   select new Tuple<A, B>(a, b);
        }

        /// <summary>
        /// Returns all elements, and the collection without that element
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static FiniteDist<Tuple<A,IEnumerable<A>>> SelectOne<A>(List<A> list)
        {
            var removedLists = list.Select(a => {
                var removedList = new List<A>(list);
                removedList.Remove(a);
                return new Tuple<A, IEnumerable<A>>(a, removedList);
               });
            return EnumUniformF(removedLists);
        }


        /// <summary>
        /// Display finite distribution as histogram
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dist"></param>
        /// <param name="showItem">Specifies a string representation of distribution item. Defaults to ToString.</param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static string Histogram<A>(this FiniteDist<A> dist, Func<A, string> showItem = null, double scale = 100)
        {
            if (showItem == null) showItem = a => a.ToString();
            return ProbabilityMonad.Histogram.Finite(dist.Explicit, showItem, scale);
        }

    }
}

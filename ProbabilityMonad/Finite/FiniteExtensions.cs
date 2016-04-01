using System;
using static ProbabilityMonad.Base;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityMonad
{
    /// <summary>
    /// Useful extension methods for finite distributions.
    /// </summary>
    public static class FiniteExtensions
    {
        /// <summary>
        /// Multiply all the probabilities by a constant so they sum to 1
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="probs"></param>
        /// <returns></returns>
        public static IEnumerable<ItemProb<A>> Normalize<A>(IEnumerable<ItemProb<A>> probs)
        {
            var norm = probs.Select(p => p.Prob.Value).Sum();
            return probs.Select(prob => new ItemProb<A>(prob.Item, prob.Prob.Div(Prob(norm))));
        }

        /// <summary>
        /// Pick a value from a distribution using a probability
        /// </summary>
        public static A Pick<A>(this FiniteDist<A> distribution, Prob pickProb)
        {
            var probVal = pickProb.Value;
            foreach (var prob in distribution.Distribution)
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
        public static Prob ProbOf<A>(this FiniteDist<A> distribution, Func<A, bool> eventTest)
        {
            return Prob(distribution.Distribution.Where(p => eventTest(p.Item)).Select(p => p.Prob.Value).Sum());
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
            return new FiniteDist<A>(Normalize(distribution.Distribution
                .Select(p => ItemProb(p.Item, likelihood(p.Item).Mult(p.Prob)))));
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
            return new FiniteDist<A>(distribution.Distribution
                .Select(p => ItemProb(p.Item, likelihood(p.Item).Mult(p.Prob))));
        }

        /// <summary>
        /// Hard reweight by a condition that depends on associated item
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="distribution"></param>
        /// <param name="likelihood"></param>
        /// <returns></returns>
        public static FiniteDist<A> ConditionHard<A>(this FiniteDist<A> distribution, Func<A, bool> likelihood)
        {
            return new FiniteDist<A>(Normalize(distribution.Distribution
                .Select(p => ItemProb(p.Item, likelihood(p.Item) ? p.Prob : Prob(0)))));
        }

        public static FiniteDist<A> Normalize<A>(this FiniteDist<A> dist)
        {
            return new FiniteDist<A>(Normalize(dist.Distribution));
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
            return EnumUniformD(removedLists);
        }

    }
}

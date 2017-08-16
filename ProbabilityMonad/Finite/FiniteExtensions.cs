using System;
using static ProbCSharp.ProbBase;
using System.Collections.Generic;
using System.Linq;

namespace ProbCSharp
{
    // Useful extension methods for finite distributions.
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
        /// Lifts a FiniteDist<A> into a SampleableDist<A>
        /// </summary>
        public static PrimitiveDist<A> ToSampleDist<A>(this FiniteDist<A> dist)
        {
            return new SampleDist<A>(() =>
            {
                var rand = new MathNet.Numerics.Distributions.ContinuousUniform().Sample();
                return dist.Pick(Prob(rand));
            });
        }

        /// <summary>
        /// Returns the probability of a certain event
        /// </summary>
        public static Prob ProbOf<A>(this FiniteDist<A> dist, Func<A, bool> eventTest)
        {
            var matches = dist.Explicit.Weights.Where(p => eventTest(p.Item));
            if (!matches.Any()) return Prob(0);
            return Prob(matches.Select(p => p.Prob.Value).Sum());
        }

        /// <summary>
        /// Reweight by a probability that depends on associated item
        /// </summary>
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
        public static FiniteDist<A> ConditionSoftUnnormalized<A>(this FiniteDist<A> distribution, Func<A, Prob> likelihood)
        {
            return new FiniteDist<A>(
                distribution.Explicit.Select(p => ItemProb(p.Item, likelihood(p.Item).Mult(p.Prob)))
            );
        }

        /// <summary>
        /// Hard reweight by a condition that depends on associated item
        /// </summary>
        public static FiniteDist<A> ConditionHard<A>(this FiniteDist<A> distribution, Func<A, bool> condition)
        {
            return new FiniteDist<A>(
                distribution.Explicit
                    .Select(p => ItemProb(p.Item, condition(p.Item) ? p.Prob : Prob(0)))
                    .Normalize()
            );
                    
                
        }

        /// <summary>
        /// Computes the posterior distribution, given a piece of data and a likelihood function
        /// </summary>
        public static FiniteDist<A> UpdateOn<A, D>(this FiniteDist<A> prior, Func<A, D, Prob> likelihood, D datum)
        {
            return prior.ConditionSoft(w => likelihood(w, datum));
        }

        /// <summary>
        /// Computes the posterior distribution, given a list of data and a likelihood function
        /// </summary>
        public static FiniteDist<A> UpdateOn<A, D>(this FiniteDist<A> prior, Func<A, D, Prob> likelihood, IEnumerable<D> data)
        {
            return data.Aggregate(prior, (dist, datum) => dist.UpdateOn(likelihood, datum));
        }

        /// <summary>
        /// Normalize a finite distribution
        /// </summary>
        public static FiniteDist<A> Normalize<A>(this FiniteDist<A> dist)
        {
            return new FiniteDist<A>(dist.Explicit.Normalize());
        }
        
        /// <summary>
        /// Join two independent distributions
        /// </summary>
        public static FiniteDist<Tuple<A,B>> Join<A, B>(this FiniteDist<A> self, FiniteDist<B> other)
        {
            return from a in self
                   from b in other
                   select new Tuple<A, B>(a, b);
        }

        /// <summary>
        /// Returns all elements, and the collection without that element
        /// </summary>
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
        /// <param name="showItem">Specifies a string representation of distribution item. Defaults to ToString.</param>
        public static string Histogram<A>(this FiniteDist<A> dist, Func<A, string> showItem = null, double scale = 100)
        {
            if (showItem == null) showItem = a => a.ToString();
            return ProbCSharp.Histogram.Finite(dist.Explicit, showItem, scale);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    /// <summary>
    /// Constructor helpers
    /// </summary>
    public static class Base
    {
        public static Prob Prob(double probability)
        {
            return new DoubleProb(probability);
        }

        public static ItemProb<A> ItemProb<A>(A item, Prob prob)
        {
            return new ItemProb<A>(item, prob);
        }
    }

    /// <summary>
    /// Tuple class since C# tuples cause a few issues
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class ItemProb<A>
    {
        public A Item { get; }
        public Prob Prob { get; }
        public ItemProb(A item, Prob prob)
        {
            Item = item;
            Prob = prob;
        }
    }

    /// <summary>
    /// Discrete distribution monad
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class FiniteDist<A>
    {
        public FiniteDist(IEnumerable<ItemProb<A>> probs)
        {
            Distribution = probs;
        }

        public FiniteDist(params ItemProb<A>[] probs)
        {
            Distribution = probs;
        }

        public IEnumerable<ItemProb<A>> Distribution { get; }
    }


    /// <summary>
    /// Extension methods for discrete distribution monad
    /// </summary>
    public static class FiniteDistExt
    {
        public static FiniteDist<B> Select<A, B>(this FiniteDist<A> self, Func<A, B> select)
        {
            return new FiniteDist<B>(self.Distribution.Select(i => new ItemProb<B>(select(i.Item), i.Prob)));
        }

        public static IEnumerable<ItemProb<A>> DistJoin<A>(FiniteDist<FiniteDist<A>> distOverDist)
        {
            foreach (var dist in distOverDist.Distribution)
            {
                foreach (var itemProb in dist.Item.Distribution)
                {
                    yield return ItemProb(
                            itemProb.Item,
                            itemProb.Prob.Mult(dist.Prob)
                    );
                }
            }
        }

        public static FiniteDist<C> SelectMany<A, B, C>(
            this FiniteDist<A> self,
            Func<A, FiniteDist<B>> bind,
            Func<A, B, C> project
        )
        {
            var itemProbs = 
                self.Distribution.Select(a => 
                    ItemProb(bind(a.Item).Select(b => 
                        project(a.Item, b)), a.Prob));

            var dists = new FiniteDist<FiniteDist<C>>(itemProbs);
            return new FiniteDist<C>(DistJoin(dists));
        }
    }

    /// <summary>
    /// Continuous distribution monad
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class ContDist<A>
    {
        public Func<A> Sample { get; }
        public ContDist(Func<A> sample)
        {
            Sample = sample;
        }
    }


    /// <summary>
    /// Extension methods for continuous dist monad
    /// </summary>
    public static class ContDiscExt
    {
        public static ContDist<B> Select<A, B>(this ContDist<A> self, Func<A, B> select)
        {
            return new ContDist<B>(() => select(self.Sample()));
        }

        public static ContDist<C> SelectMany<A, B, C>(
            this ContDist<A> self,
            Func<A, ContDist<B>> bind,
            Func<A, B, C> project
        )
        {
            return new ContDist<C>(() =>
            {
                var firstSample = self.Sample();
                var secondSample = bind(firstSample).Sample();
                return project(firstSample, secondSample);
            });
            
        }
    } 

    public class ConditionalDist<A>
    {
        public Func<A, Prob> Condition { get; }
        public ContDist<A> Dist { get; }
        public ConditionalDist(Func<A, Prob> condition, ContDist<A> dist)   
        {
            Condition = condition;
            Dist = dist;
        }
    }



}

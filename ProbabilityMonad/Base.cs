using CSharpProbabilityMonad;
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
    /// Distribution monad
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Dist<A>
    {
        public Dist(IEnumerable<ItemProb<A>> probs)
        {
            Distribution = probs;
        }

        public Dist(params ItemProb<A>[] probs)
        {
            Distribution = probs;
        }

        public IEnumerable<ItemProb<A>> Distribution { get; }
    }

    /// <summary>
    /// Extension methods for distribution monad
    /// </summary>
    public static class DistExt
    {
        public static Dist<B> Select<A, B>(this Dist<A> self, Func<A, B> select)
        {
            return new Dist<B>(self.Distribution.Select(i => new ItemProb<B>(select(i.Item), i.Prob)));
        }

        public static IEnumerable<ItemProb<A>> DistJoin<A>(Dist<Dist<A>> distOverDist)
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

        public static Dist<C> SelectMany<A, B, C>(
            this Dist<A> self,
            Func<A, Dist<B>> bind,
            Func<A, B, C> project
        )
        {
            var itemProbs = 
                self.Distribution.Select(a => 
                    ItemProb(bind(a.Item).Select(b => 
                        project(a.Item, b)), a.Prob));

            var dists = new Dist<Dist<C>>(itemProbs);
            return new Dist<C>(DistJoin(dists));
        }
    }

    public class SampleDist<A>
    {
        public Func<A> Sample { get; }
        public SampleDist(Func<A> sample)
        {
            Sample = sample;
        }
    }


    public static class SampleDistExt
    {
        public static SampleDist<B> Select<A, B>(this SampleDist<A> self, Func<A, B> select)
        {
            return new SampleDist<B>(() => select(self.Sample()));
        }

        public static SampleDist<C> SelectMany<A, B, C>(
            this SampleDist<A> self,
            Func<A, SampleDist<B>> bind,
            Func<A, B, C> project
        )
        {
            return new SampleDist<C>(() =>
            {
                var firstSample = self.Sample();
                var secondSample = bind(firstSample).Sample();
                return project(firstSample, secondSample);
            });
            
        }
    } 

}

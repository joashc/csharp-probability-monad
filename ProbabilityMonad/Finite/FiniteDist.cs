using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
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
    /// Monad instance for discrete distributions 
    /// </summary>
    public static class FiniteDistMonad
    {
        /// <summary>
        /// fmap f (FiniteDist dist) = FiniteDist $ map (first f) dist
        /// </summary>
        public static FiniteDist<B> Select<A, B>(this FiniteDist<A> self, Func<A, B> select)
        {
            return new FiniteDist<B>(self.Distribution.Select(i => 
                new ItemProb<B>(select(i.Item), i.Prob)));
        }

        /// <summary>
        /// (FiniteDist dist) >>= bind = FiniteDist $ do
        ///   (x,p) <- dist
        ///   (y,q) <- bind x
        ///   return (y,p*q)
        /// </summary>
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

        /// <summary>
        /// Take the cartensian product, multiplying out probabilities
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="distOverDist"></param>
        /// <returns></returns>
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

    }

}

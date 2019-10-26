using System;
using System.Linq;
using static ProbCSharp.ProbBase;

namespace ProbCSharp
{
    /// <summary>
    /// Discrete distribution monad
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class FiniteDist<A>
    {
        public FiniteDist(Samples<A> samples)
            => Explicit = samples;

        public FiniteDist(params ItemProb<A>[] samples)
            => Explicit = Samples(samples);

        public Samples<A> Explicit { get; }
    }


    // Monad instance for discrete distributions 
    public static class FiniteDistMonad
    {
        /// <summary>
        /// fmap f (FiniteDist (Samples xs)) = FiniteDist $ Samples $ map (first f) xs
        /// </summary>
        public static FiniteDist<B> Select<A, B>(this FiniteDist<A> self, Func<A, B> select)
            => new FiniteDist<B>(Samples(self.Explicit.Weights.Select(i =>
                ItemProb(@select(i.Item), i.Prob))));

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
            var itemProbs = from xp in self.Explicit.Weights
                            from yq in bind(xp.Item).Explicit.Weights
                            select ItemProb(project(xp.Item, yq.Item), xp.Prob.Mult(yq.Prob));

            return new FiniteDist<C>(Samples(itemProbs));
        }
    }

}

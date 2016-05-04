using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbCSharp.ProbBase;

namespace ProbCSharp
{
    // Extension methods for prior
    public static class PriorWeightedExtensions
    {
        public static Dist<ItemProb<A>> WeightedPrior<A>(this Dist<A> dist)
        {
            return dist.Run(new PriorWeighted<A>());
        }
    }

    /// <summary>
    /// Allows sampling from prior, weighted by likelihood scores of conditionals
    /// We do *not* remove conditionals from the right of a bind.
    /// This is so that the presence of a conditional is independent of random choices in the model.
    /// </summary>
    public class PriorWeighted<A> : DistInterpreter<A, Dist<ItemProb<A>>>
    {
        public Dist<ItemProb<A>> Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            return from x in dist.Run(new PriorWeighted<B>())
                   from y in bind(x.Item) // Don't remove conditionals here
                   select new ItemProb<A>(y, x.Prob);
        }

        public Dist<ItemProb<A>> Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            return from itemProb in dist.Run(new PriorWeighted<A>())
            select new ItemProb<A>(itemProb.Item, itemProb.Prob.Mult(lik(itemProb.Item)));
        }

        public DistInterpreter<B, Y> New<B, Y>()
        {
            return new PriorWeighted<B>() as DistInterpreter<B, Y>;
        }

        public Dist<ItemProb<A>> Primitive(PrimitiveDist<A> dist)
        {
            return new Primitive<ItemProb<A>>(dist.Select(a => ItemProb(a, Prob(1))));
        }

        public Dist<ItemProb<A>> Pure(A value)
        {
            return new Pure<ItemProb<A>>(new ItemProb<A>(value, Prob(1)));
        }
    }

}

using System;
using System.Linq;
using System.Collections.Generic;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    public static class InterpretersExt
    {
        public static Dist<ItemProb<A>> Prior<A>(this Dist<A> dist)
        {
            return dist.Run(new PriorInterpreter<A>());
        }

        public static A  Sample<A>(this Dist<A> dist)
        {
            return dist.Run(new Sampler<A>());
        }

        public static IEnumerable<A> SampleN<A>(this Dist<A> dist, int n)
        {
            return Enumerable.Range(0, n).Select(_ => dist.Sample());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class PriorInterpreter<A> : DistInterpreter<A, Dist<ItemProb<A>>>
    {
        public Dist<ItemProb<A>> Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            return from x in dist.Run(new PriorInterpreter<B>())
                   from y in bind(x.Item).Run(new PriorInterpreter<A>())
                   select new ItemProb<A>(y.Item, x.Prob);
        }

        public Dist<ItemProb<A>> Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            return from itemProb in dist.Run(new PriorInterpreter<A>())
            select new ItemProb<A>(itemProb.Item, itemProb.Prob.Mult(lik(itemProb.Item)));
        }

        public Dist<ItemProb<A>> Primitive(ContDist<A> dist)
        {
            return new Pure<ItemProb<A>>(new ItemProb<A>(dist.Sample(), Prob(1)));
        }

        public Dist<ItemProb<A>> Pure(A value)
        {
            return new Pure<ItemProb<A>>(new ItemProb<A>(value, Prob(1)));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class PriorDiscard<A> : DistInterpreter<A, Dist<A>>
    {
        Dist<A> DistInterpreter<A, Dist<A>>.Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            return new Bind<B, A>(dist.Run(new PriorDiscard<B>()), bind);
        }

        Dist<A> DistInterpreter<A, Dist<A>>.Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            return dist.Run(new PriorDiscard<A>());
        }

        Dist<A> DistInterpreter<A, Dist<A>>.Primitive(ContDist<A> dist)
        {
            return new Pure<A>(dist.Sample());
        }

        Dist<A> DistInterpreter<A, Dist<A>>.Pure(A value)
        {
            return new Pure<A>(value);
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Sampler<A> : DistInterpreter<A, A>
    {
        A DistInterpreter<A, A>.Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            var x = dist.Run(new Sampler<B>());
            return bind(x).Run(new Sampler<A>());
        }

        A DistInterpreter<A, A>.Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            throw new ArgumentException("Cannot sample from conditional distribution.");
        }

        A DistInterpreter<A, A>.Primitive(ContDist<A> dist)
        {
            return dist.Sample();
        }

        A DistInterpreter<A, A>.Pure(A value)
        {
            return value;
        }
    }

}

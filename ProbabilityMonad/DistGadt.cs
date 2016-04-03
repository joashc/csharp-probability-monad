using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    /// <summary>
    /// GADT for representing distributions as free monads.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public interface Dist<A>
    {
        X Run<X>(DistInterpreter<A, X> interpreter);
    }

    /// <summary>
    /// Interpreter interface. 
    /// Greatly indebted to Kennedy and Russo (2005)
    /// http://research.microsoft.com/pubs/64040/gadtoop.pdf
    /// for the idea of using the visitor pattern to implement GADTS.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="X"></typeparam>
    public interface DistInterpreter<A, X>
    {
        X Pure(A value);
        X Primitive(ContDist<A> dist);
        X Conditional(Func<A, Prob> lik, Dist<A> dist);
        X Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind);
    }

    /// <summary>
    /// This is basically `return` (the Haskell return)
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Pure<A> : Dist<A>
    {
        public readonly A Value;
        public Pure(A value)
        {
            Value = value;
        }

        public X Run<X>(DistInterpreter<A, X> interpreter)
        {
            return interpreter.Pure(Value);
        }
    }

    /// <summary>
    /// Primitive distribution
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Primitive<A> : Dist<A>
    {
        public readonly ContDist<A> dist;
        public Primitive(ContDist<A> dist)
        {
            this.dist = dist;
        }

        public X Run<X>(DistInterpreter<A, X> interpreter)
        {
            return interpreter.Primitive(dist);
        }
    }

    /// <summary>
    /// Conditional 
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Conditional<A> : Dist<A>
    {
        public readonly Func<A, Prob> likelihood;
        public readonly Dist<A> dist;
        public Conditional(Func<A, Prob> likelihood, Dist<A> dist)
        {
            this.likelihood = likelihood;
            this.dist = dist;
        }

        public X Run<X>(DistInterpreter<A, X> interpreter)
        {
            return interpreter.Conditional(likelihood, dist);
        }
    }

    /// <summary>
    /// Bind type. Equivalent to Free f a
    /// </summary>
    /// <typeparam name="Y"></typeparam>
    /// <typeparam name="A"></typeparam>
    public class Bind<Y, A> : Dist<A>
    {
        public readonly Dist<Y> dist;
        public readonly Func<Y, Dist<A>> bind;
        public Bind(Dist<Y> dist, Func<Y, Dist<A>> bind)
        {
            this.dist = dist;
            this.bind = bind;
        }

        public X Run<X>(DistInterpreter<A, X> interpreter)
        {
            return interpreter.Bind(dist, bind);
        }
    }

    /// <summary>
    /// The "bind" operation here just wraps in another Bind type.
    /// We join without renormalization, so it's a Free Monad.
    /// </summary>
    public static class DistExt
    {
        public static Dist<B> Select<A, B>(this Dist<A> dist, Func<A, B> f)
        {
            return new Bind<A, B>(dist, a => new Pure<B>(f(a)));
        }

        public static Dist<B> SelectMany<A, B>(this Dist<A> dist, Func<A, Dist<B>> bind)
        {
            return new Bind<A, B>(dist, bind);
        }

        public static Dist<C> SelectMany<A, B, C>(
            this Dist<A> dist,
            Func<A, Dist<B>> bind,
            Func<A, B, C> project
        )
        {
            return 
                new Bind<A, C>(dist, a =>
                   new Bind<B, C>(bind(a), b =>
                       new Pure<C>(project(a, b))
                    )
                );
        }


        /// <summary>
        /// `sequence` can be implemented as
        /// sequence xs = foldr (liftM2 (:)) (return []) xs
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dists"></param>
        /// <returns></returns>
        public static Dist<IEnumerable<A>> Sequence<A>(this IEnumerable<Dist<A>> dists)
        {
            return dists.Aggregate(
                Return<IEnumerable<A>>(new List<A>()),
                (listDist, aDist) => from a in aDist
                                     from list in listDist
                                     select Append(list, a));
        }
    }

}

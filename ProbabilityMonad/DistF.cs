using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityMonad
{
    /// <summary>
    /// data DistOp a next
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="Next"></typeparam>
    public abstract class DistOp<A, Next>
    {
        public abstract X Match<X>(
            Func<ContDist<A>, Func<ContDist<A>, Next>, X> primitive,
            Func<Func<string, Next>, X> getMessage
        );

        /// <summary>
        /// liftF
        /// </summary>
        public DistF<A, Next> Lift
        {
            get
            {
                return DistF<A, Next>.free(this.Select(DistF<A, Next>.pure));
            }

        }

        /// <summary>
        /// Primitive (ContDist a) next
        /// </summary>
        internal class Primitive : DistOp<A, Next>
        {
            public readonly ContDist<A> dist;
            public readonly Func<ContDist<A>, Next> next;
            public Primitive(ContDist<A> dist, Func<ContDist<A>, Next> next)
            {
                this.dist = dist;
                this.next = next;
            }

            public override X Match<X>(
                Func<ContDist<A>, Func<ContDist<A>, Next>, X> primitive,
                Func<Func<string, Next>, X> getMessage)
            {
                return primitive(dist, next);
            }
        }

        /// <summary>
        /// GetMessage (m -> next)
        /// </summary>
        internal class GetMessage : DistOp<A, Next>
        {
            public readonly Func<string, Next> f;
            public GetMessage(Func<string, Next> f)
            {
                this.f = f;
            }

            public override X Match<X>(
                Func<ContDist<A>, Func<ContDist<A>, Next>, X> primitive,
                Func<Func<string, Next>, X> getMessage)
            {
                return getMessage(f);
            }
        }
    }

    /// <summary>
    /// instance Functor (Dist a) where
    /// </summary>
    public static class DistOpFunctor
    {
        public static DistOp<A, NextB> Select<A, NextA, NextB>(this DistOp<A, NextA> self, Func<NextA, NextB> f)
        {
            return self.Match<DistOp<A, NextB>>(
                (s, n) => new DistOp<A, NextB>.Primitive(s, d => f(n(d))),
                g => new DistOp<A, NextB>.GetMessage(s => f(g(s)))
            );
        }
    }

    /// <summary>
    /// Free (DistOp a)
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="Next"></typeparam>
    public abstract class DistF<A, Next>
    {
        public abstract X Match<X>(
            Func<Next, X> done,
            Func<DistOp<A, DistF<A, Next>>, X> more
        );

        /// <summary>
        /// Free DistOp (Free (DistOp a))
        /// </summary>
        internal class Free : DistF<A, Next>
        {
            public readonly DistOp<A, DistF<A, Next>> fa;
            public Free(DistOp<A, DistF<A, Next>> fa)
            {
                this.fa = fa;
            }

            public override X Match<X>(
                Func<Next, X> done,
                Func<DistOp<A, DistF<A, Next>>, X> free)
            {
                return free(fa);
            }
        }

        /// <summary>
        /// Free constructor
        /// </summary>
        /// <param name="fa"></param>
        /// <returns></returns>
        public static DistF<A, Next> free(DistOp<A, DistF<A, Next>> fa)
        {
            return new Free(fa);
        }

        /// <summary>
        /// Pure a
        /// </summary>
        internal class Pure : DistF<A, Next>
        {
            public readonly Next next;
            public Pure(Next next)
            {
                this.next = next;
            }

            public override X Match<X>(
                Func<Next, X> pure,
                Func<DistOp<A, DistF<A, Next>>, X> more)
            {
                return pure(next);
            }
        }

        /// <summary>
        /// Pure constructor
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public static DistF<A, Next> pure(Next next)
        {
            return new Pure(next);
        }
    }

    /// <summary>
    /// instance Monad (Free (DistF a))
    /// </summary>
    public static class DistFMonad
    {
        public static DistF<A, NextB> Select<A, NextA, NextB>(
            this DistF<A, NextA> self,
            Func<NextA, NextB> f)
        {
            return self.Match(
                next => DistF<A, NextB>.pure(f(next)),
                a => DistF<A, NextB>.free(a.Select(k => k.Select(f)))
            );
        }

        public static DistF<A, NextB> SelectMany<A, NextA, NextB>(
            this DistF<A, NextA> self,
            Func<NextA, DistF<A, NextB>> bind
        )
        {
            return self.Match(
                bind,
                a => DistF<A, NextB>.free(a.Select(k => k.SelectMany(bind)))
            );
        }

        public static DistF<A, NextC> SelectMany<A, NextA, NextB, NextC>(
            this DistF<A, NextA> self,
            Func<NextA, DistF<A, NextB>> bind,
            Func<NextA, NextB, NextC> project
        )
        {
            return SelectMany(self, a => Select(bind(a), b => project(a, b)));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class DistOps
    {
        public static DistF<double, ContDist<double>> Normal(double mean, double var)
        {
            return new DistOp<double, ContDist<double>>.Primitive(Distributions.Normal(mean, var), d => d).Lift;
        }

        public static DistF<A, string> GetMessage<A>()
        {
            return new DistOp<A, string>.GetMessage(s => s).Lift;
        }
    }

    /// <summary>
    /// () :: ()
    /// </summary>
    public struct Unit
    {
        public static readonly Unit Value = new Unit();
    }

    /// <summary>
    /// interpreter :: DistFMonad () -> IO ()
    /// </summary>
    public static class DistFInterpreter
    {
        public static NextA Interpret<A, NextA>(this DistF<A, NextA> self)
        {
            return self.Match(
                a => a,
                a => a.Match(
                    (s, next) =>
                    {
                        return Interpret(next(s));
                    },
                    next =>
                    {
                        var s = Console.ReadLine();
                        return Interpret(next(s));
                    }
                )
            );
        }
    }

}

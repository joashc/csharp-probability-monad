using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityMonad
{
    public interface IExpVisitor<T, R>
    {
        R VisitLit(Lit e) where T=int;
        R VisitPlus(Plus e) where T=int;
        R VisitEquals(Equals e) where T=bool;
        R VisitCond<A>(Cond<A> e) where T=A;
        R VisitTuple<A, B>(Tuple<A, B> e) where T=Pair<A, B>;
        R VisitFst<A, B>(Fst<A, B> e) where T=A;
    }
    public abstract class Exp<T>
    { ...
public abstract R Accept<R>(IExpVisitor<T, R> v);
        public T Eval() { return Accept(new EvalVisitor<T>();)}
    }
    public class Lit : Exp<int>
    { ...
public override R Accept<R>(IExpVisitor<int, R> v)
        { return v.VisitLit(this); }
    }
    public class Plus : Exp<int>
    { ...
public override R Accept<R>(IExpVisitor<int, R> v)
        {
            { return v.VisitPlus(this); }
        }
        public class Equals : Exp<int> { ...similar to Plus...}
        public class Cond<T> : Exp<T>
        { ...
public override R Accept<R>(IExpVisitor<T, R> v)
            {
                { return v.VisitCond<T>(this); }
            }
            public class Tuple<A, B> : Exp<Pair<A, B>>
            { ...
public override R Accept<R>
(IExpVisitor<Pair<A, B>, R> v)
                { return v.VisitTuple(this); }
            }
            public class Fst<A, B> : Exp<A>
            { ...
public override R Accept<R>(IExpVisitor<A, R> v)
                { return v.VisitFst(this); }
            }
            public class EvalVisitor<T> : IExpVisitor<T, T>
            {
                public T VisitLit(Lit e) { return e.value; }
                public T VisitPlus(Plus e)
                { return e.e1.Eval() + e.e2.Eval(); }
                public T VisitEquals(Equals e)
                { return e.e1.Eval() == e.e2.Eval(); }
                public T VisitCond<A>(Cond<A> e)
                { return e.e1.Eval() ? e.e2.Eval() : e.e3.Eval(); }
                public T VisitTuple<A, B>(Tuple<A, B> e)
                { return new Pair<A, B>(e.e1.Eval(), e.e2.Eval()); }
                public T VisitFst<A, B>(Fst<A, B> e)
                { return e.e.Eval().fst; }
            }

        }

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
    /// GADT for representing distributions as operational monad
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public interface Dist<A>
    {
        X Run<X>(DistInterpreter<A, X> interpreter);
        X RunParallel<X>(ParallelDistInterpreter<A, X> interpreter);
    }

    /// <summary>
    /// Interpreter interface. 
    /// Greatly indebted to Kennedy and Russo (2005)
    /// http://research.microsoft.com/pubs/64040/gadtoop.pdf
    /// for the idea of using the visitor pattern to implement GADTS.
    /// </summary>
    /// <typeparam name="A">Sample type of distribution</typeparam>
    /// <typeparam name="X"></typeparam>
    public interface DistInterpreter<A, X>
    {
        X Pure(A value);
        X Primitive(ContDist<A> dist);
        X Conditional(Func<A, Prob> lik, Dist<A> dist);
        X Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind);
        DistInterpreter<B, Y> New<B, Y>();
    }

    /// <summary>
    /// Parallel interpreter interface
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="X"></typeparam>
    public interface ParallelDistInterpreter<A, X>
    {
        X Pure(A value);
        X Primitive(ContDist<A> dist);
        X Conditional(Func<A, Prob> lik, Dist<A> dist);
        X Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind);
        X Independent(Dist<A> independent);
        X RunIndependent<T1, T2>(Dist<T1> distB, Dist<T2> distC, Func<T1, T2, Dist<A>> run);
        X RunIndependent3<T1, T2, T3>(Dist<T1> first, Dist<T2> second, Dist<T3> third, Func<T1, T2, T3, Dist<A>> run);
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

        public X RunParallel<X>(ParallelDistInterpreter<A, X> interpreter)
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

        public X RunParallel<X>(ParallelDistInterpreter<A, X> interpreter)
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

        public X RunParallel<X>(ParallelDistInterpreter<A, X> interpreter)
        {
            return interpreter.Conditional(likelihood, dist);
        }
    }

    /// <summary>
    /// Evaluate three independent distributions in parallel
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="A"></typeparam>
    public class RunIndependent3<T1, T2, T3, A> : Dist<A>
    {
        public readonly Dist<T1> first;
        public readonly Dist<T2> second;
        public readonly Dist<T3> third;
        public readonly Func<T1, T2, T3, Dist<A>> run;
        public RunIndependent3(Dist<T1> first, Dist<T2> second, Dist<T3> third, Func<T1, T2, T3, Dist<A>> run)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.run = run;
        }

        // Run sequentially if we're not using a parallel interpreter
        public X Run<X>(DistInterpreter<A, X> interpreter)
        {
            var resultA = from x in first
                          from y in second
                          from z in third
                          from result in run(x, y, z)
                          select result;
            return resultA.Run(interpreter);
        }

        public X RunParallel<X>(ParallelDistInterpreter<A, X> interpreter)
        {
            return interpreter.RunIndependent3(first, second, third, run);
        }
    }

    /// <summary>
    /// Evaluate two independent distributions in parallel
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="A"></typeparam>
    public class RunIndependent<T1, T2, A> : Dist<A>
    {
        public readonly Dist<T1> first;
        public readonly Dist<T2> second;
        public readonly Func<T1, T2, Dist<A>> run;
        public RunIndependent(Dist<T1> first, Dist<T2> second, Func<T1, T2, Dist<A>> run)
        {
            this.first = first;
            this.second = second;
            this.run = run;
        }

        // Run sequentially if we're not using a parallel interpreter
        public X Run<X>(DistInterpreter<A, X> interpreter)
        {
            var resultA = from x in first
                          from y in second
                          from result in run(x, y)
                          select result;
            return resultA.Run(interpreter);
        }

        public X RunParallel<X>(ParallelDistInterpreter<A, X> interpreter)
        {
            return interpreter.RunIndependent(first, second, run);
        }
    }

    /// <summary>
    /// Marks independent distributions for parallel evaluation
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    public class Independent<A> : Dist<Dist<A>>
    {
        public readonly Dist<A> dist;
        public Independent(Dist<A> dist)
        {
            this.dist = dist;
        }

        // Run sequentially if we're not using a parallel interpreter
        public X Run<X>(DistInterpreter<Dist<A>, X> interpreter)
        {
            return Return(dist).Run(interpreter);
        }

        public X RunParallel<X>(ParallelDistInterpreter<Dist<A>, X> interpreter)
        {
            return interpreter.Independent(Return(dist));
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

        public X RunParallel<X>(ParallelDistInterpreter<A, X> interpreter)
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
        /// Default to using recursion depth limit of 100
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dists"></param>
        /// <returns></returns>
        public static Dist<IEnumerable<A>> Sequence<A>(this IEnumerable<Dist<A>> dists)
        {
            return SequenceWithDepth(dists, 100);
        }

        /// <summary>
        /// This implementation sort of does trampolining to avoid stack overflows,
        /// but for performance reasons it recursively divides the list
        /// into groups up to a recursion depth, instead of trampolining every iteration.
        ///
        /// This should limit the recursion depth to around 
        /// $$s\log_{s}{n}$$
        /// where s is the specified recursion depth limit
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dists"></param>
        /// <returns></returns>
        public static Dist<IEnumerable<A>> SequenceWithDepth<A>(this IEnumerable<Dist<A>> dists, int recursionDepth)
        {
            var sections = dists.Count() / recursionDepth;
            if (sections <= 1) return RunSequence(dists);
            return from nested in SequenceWithDepth(SequencePartial(dists, recursionDepth), recursionDepth)
                   select nested.SelectMany(a => a);
        }

        /// <summary>
        /// `sequence` can be implemented as
        /// sequence xs = foldr (liftM2 (:)) (return []) xs
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dists"></param>
        /// <returns></returns>
        private static Dist<IEnumerable<A>> RunSequence<A>(IEnumerable<Dist<A>> dists)
        {
            return dists.Aggregate(
                Return<IEnumerable<A>>(new List<A>()),
                (listDist, aDist) => from a in aDist
                                     from list in listDist
                                     select Append(list, a)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="dists"></param>
        /// <param name="groupSize"></param>
        /// <returns></returns>
        private static IEnumerable<Dist<IEnumerable<A>>> SequencePartial<A>(IEnumerable<Dist<A>> dists, int groupSize)
        {
            var numGroups = dists.Count() / groupSize;
            return Enumerable.Range(0, numGroups)
                             .Select(groupNum => RunSequence(dists.Skip(groupNum * groupSize).Take(groupSize)));
        }

    }

}
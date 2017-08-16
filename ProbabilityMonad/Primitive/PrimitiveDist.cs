using System;

namespace ProbCSharp
{
    /// <summary>
    /// A distribution that can be sampled from.
    /// These distributions are "primitive" in that they can only be composed with other primitive distributions.
    /// Lifting this into the Dist<A> GADT allows free composition with any distribution.
    /// </summary>
    public interface PrimitiveDist<A>
    {
        Func<A> Sample { get; }
    }

    /// <summary>
    /// Construct a primitive distribution from a sampling function
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class SampleDist<A> : PrimitiveDist<A>
    {
        private readonly Func<A> _sample;
        public SampleDist(Func<A> sample)
        {
            _sample = sample;
        }

        public Func<A> Sample
        {
            get { return _sample; }
        }
    }


    // Primitive distribution monad instance
    public static class PrimitiveDistMonad
    {
        public static PrimitiveDist<B> Select<A, B>(this PrimitiveDist<A> self, Func<A, B> f)
        {
            return new SampleDist<B>(() => f(self.Sample()));
        }

        public static PrimitiveDist<C> SelectMany<A, B, C>(
            this PrimitiveDist<A> self,
            Func<A, PrimitiveDist<B>> bind,
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

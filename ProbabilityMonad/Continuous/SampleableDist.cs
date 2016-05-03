using System;
using System.Collections.Generic;

namespace ProbCSharp
{
    /// <summary>
    /// A distribution that can be sampled from
    /// </summary>
    public interface SampleableDist<A>
    {
        Func<A> Sample { get; }
    }

    /// <summary>
    /// Lifts a sampling function into a sampleable distribution
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class SampleDist<A> : SampleableDist<A>
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


    // Sampleable distribution monad instance
    public static class SampleableDistMonad
    {
        public static SampleableDist<B> Select<A, B>(this SampleableDist<A> self, Func<A, B> f)
        {
            return new SampleDist<B>(() => f(self.Sample()));
        }

        public static SampleableDist<C> SelectMany<A, B, C>(
            this SampleableDist<A> self,
            Func<A, SampleableDist<B>> bind,
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

using System;

namespace ProbabilityMonad
{
    /// <summary>
    /// Continuous distribution
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public interface ContDist<A>
    {
        Func<A> Sample { get; }
    }

    /// <summary>
    /// Most basic continuous distribution.
    /// Almost like a monad return
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class ContDistImpl<A> : ContDist<A>
    {
        private readonly Func<A> _sample;
        public ContDistImpl(Func<A> sample)
        {
            _sample = sample;
        }

        public Func<A> Sample
        {
            get { return _sample; }
        }
    }


    /// <summary>
    /// Continuous dist monad instance
    /// </summary>
    public static class ContDistMonad
    {
        public static ContDist<B> Select<A, B>(this ContDist<A> self, Func<A, B> f)
        {
            return new ContDistImpl<B>(() => f(self.Sample()));
        }

        public static ContDist<C> SelectMany<A, B, C>(
            this ContDist<A> self,
            Func<A, ContDist<B>> bind,
            Func<A, B, C> project
        )
        {
            return new ContDistImpl<C>(() =>
            {
                var firstSample = self.Sample();
                var secondSample = bind(firstSample).Sample();
                return project(firstSample, secondSample);
            });
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad
{
    /// <summary>
    /// Sequential Monte Carlo
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class SMC<A> : DistInterpreter<A, Dist<Samples<A>>>
    {
        public int n;
        public SMC(int n)
        {
            this.n = n;
        }

        public Dist<Samples<A>> Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            return from ps in dist.Run(new SMC<B>(n))
                   let unzipped = ps.Unzip()
                   from ys in unzipped.Item1.Select(bind).Sequence()
                   select Samples(ys.Zip(unzipped.Item2, ItemProb));
        }

        public Dist<Samples<A>> Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            var updated = new Conditional<Samples<A>>(
                samples => Prob(samples.Select(s => s.Prob.Value).Sum()),
                    from ps in dist.Run(new SMC<A>(n))
                    select ps.Select(ip => ItemProb(ip.Item, lik(ip.Item).Mult(ip.Prob)))
                );

            return from samples in updated
                   from resampled in Importance.Resample(samples)
                   select Importance.Normalize(resampled);
        }

        public Dist<Samples<A>> Primitive(ContDist<A> dist)
        {
            var d = new Primitive<ItemProb<A>>(new ContDistImpl<ItemProb<A>>(() => ItemProb(dist.Sample(), Prob(1))));
            return Enumerable.Repeat(d, n).Sequence().Select(Samples);
        }

        public Dist<Samples<A>> Pure(A value)
        {
            var d = Return(ItemProb(value, Prob(1)));
            return Enumerable.Repeat(d, n).Sequence().Select(Samples);
        }
    }
}

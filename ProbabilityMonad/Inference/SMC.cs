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
    public class SMC<A> : DistInterpreter<A, Dist<IEnumerable<ItemProb<A>>>>
    {
        public int n;
        public SMC(int n)
        {
            this.n = n;
        }

        public Dist<IEnumerable<ItemProb<A>>> Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            return from ps in dist.Run(new SMC<B>(n))
                   let unzipped = ps.Unzip()
                   from ys in unzipped.Item1.Select(bind).Sequence()
                   select ys.Zip(unzipped.Item2, ItemProb);
        }

        public Dist<IEnumerable<ItemProb<A>>> Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            var updated = new Conditional<IEnumerable<ItemProb<A>>>(
                samples => Prob(samples.Select(s => s.Prob.Value).Sum()),
                    from ps in dist.Run(new SMC<A>(n))
                    select ps.Select(ip => ItemProb(ip.Item, lik(ip.Item).Mult(ip.Prob)))
                );

            return from samples in updated
                   from resampled in Importance.Resample(samples)
                   select Importance.Normalize(resampled);
        }

        public Dist<IEnumerable<ItemProb<A>>> Primitive(ContDist<A> dist)
        {
            var d = from sample in new Primitive<A>(dist)
                    select ItemProb(sample, Prob(1));
            return Enumerable.Repeat(d, n).Sequence();
        }

        public Dist<IEnumerable<ItemProb<A>>> Pure(A value)
        {
            return Return<IEnumerable<ItemProb<A>>>(new List<ItemProb<A>> { ItemProb(value, Prob(1)) });
        }
    }
}

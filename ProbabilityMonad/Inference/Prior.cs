using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProbCSharp.ProbBase;

namespace ProbCSharp
{
    /// <summary>
    /// Draw samples from prior and discard likelihood scores
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Prior<A> : DistInterpreter<A, Dist<A>>
    {
        public DistInterpreter<B, Y> New<B, Y>()
        {
            return new Prior<B>() as DistInterpreter<B,Y>;
        }

        Dist<A> DistInterpreter<A, Dist<A>>.Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
        {
            return new Bind<B, A>(dist.Run(new Prior<B>()), bind);
        }

        Dist<A> DistInterpreter<A, Dist<A>>.Conditional(Func<A, Prob> lik, Dist<A> dist)
        {
            return dist.Run(new Prior<A>());
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


}

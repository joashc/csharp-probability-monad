using System;

namespace ProbCSharp
{
    /// <summary>
    /// Draw samples from prior and discard likelihood scores
    /// </summary>
    public class Prior<A> : DistInterpreter<A, Dist<A>>
    {
        public DistInterpreter<B, Y> New<B, Y>()
            => new Prior<B>() as DistInterpreter<B,Y>;

        Dist<A> DistInterpreter<A, Dist<A>>.Bind<B>(Dist<B> dist, Func<B, Dist<A>> bind)
            => new Bind<B, A>(dist.Run(new Prior<B>()), bind);

        Dist<A> DistInterpreter<A, Dist<A>>.Conditional(Func<A, Prob> lik, Dist<A> dist)
            => dist.Run(new Prior<A>());

        Dist<A> DistInterpreter<A, Dist<A>>.Primitive(PrimitiveDist<A> dist)
            => new Pure<A>(dist.Sample());

        Dist<A> DistInterpreter<A, Dist<A>>.Pure(A value)
            => new Pure<A>(value);
    }


}

namespace ProbabilityMonad
{
    /// <summary>
    /// Tuple class since C# tuples cause a few issues
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class ItemProb<A>
    {
        public A Item { get; }
        public Prob Prob { get; }
        public ItemProb(A item, Prob prob)
        {
            Item = item;
            Prob = prob;
        }
    }
}

namespace ProbCSharp
{
    /// <summary>
    /// Represents an item and associated probability
    /// </summary>
    public class ItemProb<A>
    {
        public A Item { get; }
        public Prob Prob { get; }

        public ItemProb(A item, Prob prob)
            => (Item, Prob) = (item, prob);

        public override string ToString()
            => $"ItemProb ({Item}, {Prob})";
    }
}

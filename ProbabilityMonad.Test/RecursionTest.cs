using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProbabilityMonad.Test.Models;
using static ProbabilityMonad.Base;
using System.Diagnostics;

namespace ProbabilityMonad.Test
{
    [TestClass]
    public class RecursionTest
    {

        /// <summary>
        /// This will pass a very long list to Sequence().
        /// A naive implementation of sequence overflows the stack with list lengths of around 1,000-10,000.
        /// We use an implementation of sequence that places a logarithmic bound on the recursion depth.
        /// </summary>
        [TestMethod]
        public void SmcRecursion()
        {
            var conditionalDie = Dice.ConditionalDie(2);
            var smcResult = conditionalDie.SmcMultiple(100000, 5).Sample().Select(ip => ItemProb((double) ip.Item, ip.Prob));
        }
    }
}

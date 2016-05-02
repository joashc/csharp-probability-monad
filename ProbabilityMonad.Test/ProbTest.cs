using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace ProbabilityMonad.Test
{
    [TestClass]
    public class ProbTest
    {
        [TestMethod]
        public void DoubleProb_Multiplies()
        {
            var d1 = new DoubleProb(0.5);
            var d2 = new DoubleProb(0.5);
            var sum = d1.Mult(d2);
            Assert.AreEqual("25%", sum.ToString());
        }

        [TestMethod]
        public void LogProb_Multiplies()
        {
            var d1 = new LogProb(Math.Log(0.5));
            var d2 = new LogProb(Math.Log(0.5));
            var sum = d1.Mult(d2);
            Assert.AreEqual("25%", sum.ToString());
        }

        [TestMethod]
        public void LogProb_Divides()
        {
            var d1 = new LogProb(Math.Log(0.5));
            var d2 = new LogProb(Math.Log(0.8));
            var sum = d1.Div(d2);
            Assert.AreEqual("62.5%", sum.ToString());
        }
    }
}

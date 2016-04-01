using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad.Test
{

    [TestClass]
    public class BayesianNetworkTest
    {
        internal class SprinklerEvent
        {
            public SprinklerEvent(bool raining, bool sprinkler, bool grassWet)
            {
                Raining = raining;
                Sprinkler = sprinkler;
                GrassWet = grassWet;
            }

            public bool Raining { get; }
            public bool Sprinkler { get; }
            public bool GrassWet { get; }
        }

        [TestMethod]
        public void SprinklerModel()
        {
            // Truth table for grass being wet
            Func<bool, bool, Prob> wetProb = (rain, sprinkler) =>
            {
                if (rain && sprinkler) return Prob(0.98);
                if (rain && !sprinkler) return Prob(0.8);
                if (!rain && sprinkler) return Prob(0.9);
                return Prob(0);
            };
            var rainDist = Bernoulli(Prob(0.2));

            // Bayesian network for sprinkler model
            var sprinklerModel =
                 from rain in rainDist
                 from sprinkler in Bernoulli(Prob(rain ? 0.01 : 0.4))
                 from wet in Bernoulli(wetProb(rain, sprinkler))
                 select new SprinklerEvent(rain, sprinkler, wet);

            // Probability of raining should be unaffected
            var prRained = sprinklerModel.ProbOf(e => e.Raining);
            Assert.AreEqual("20%", prRained.ToString());

            var prRainedAndWet = sprinklerModel.ProbOf(e => e.Raining && e.GrassWet);
            Assert.AreEqual("16%", prRainedAndWet.ToString());

            // Condition on grass being wet
            var givenGrassWet = sprinklerModel.ConditionHard(e => e.GrassWet);
            var prRainingGivenWet = givenGrassWet.ProbOf(e => e.Raining);

            Assert.AreEqual("35.7%", prRainingGivenWet.ToString());

        }

        internal class Door 
        {
            public string Name { get; }
            public Door(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return $"Door {Name}";
            }
        }

        internal class MontyHallState
        {
            public MontyHallState(Door winner, Door picked)
            {
                Winner = winner;
                Picked = picked;
            }

            public Door Winner { get; }
            public Door Picked { get; }
        }

        [TestMethod]
        public void MontyHall()
        {
            // Monty hall without switching
            Func<List<Door>, FiniteDist<MontyHallState>> 
            montyHall = doors =>
                from winner in EnumUniformD(doors)
                from picked in EnumUniformD(doors)
                select new MontyHallState(winner, picked);

            // Host has equal chance of opening any non-winning door you didn't pick
            Func<List<Door>, Door, Door, FiniteDist<Door>> 
            Open = (doors, winner, picked) => EnumUniformD(doors.Where(d => d != picked && d != winner));

            // Monty hall with switching
            Func<List<Door>, FiniteDist<MontyHallState>>
            switched = doors =>
                from initial in montyHall(doors)
                from opened in Open(doors, initial.Winner, initial.Picked)
                // Pick one of the closed doors you didn't pick at first
                from newPick in EnumUniformD(doors.Where(d => d != initial.Picked && d != opened))
                select new MontyHallState(initial.Winner, newPick);

            // You won if you picked the winner 
            Func<MontyHallState, bool> won = s => s.Picked == s.Winner;

            // Three door monty hall 
            var threeDoors = new List<Door> {
                new Door("1"),
                new Door("2"),
                new Door("3")
            };
            var prStayWon = montyHall(threeDoors).ProbOf(won);
            Assert.AreEqual("33.3%", prStayWon.ToString());
            var prSwitchWon = switched(threeDoors).ProbOf(won);
            Assert.AreEqual("66.6%", prSwitchWon.ToString());

            // Four door monty hall
            var fourDoors = new List<Door> {
                new Door("1"),
                new Door("2"),
                new Door("3"),
                new Door("4")
            };

            var prStayWon4 = montyHall(fourDoors).ProbOf(won);
            Assert.AreEqual("25%", prStayWon4.ToString());
            var prSwitchWon4 = switched(fourDoors).ProbOf(won);
            Assert.AreEqual("37.5%", prSwitchWon4.ToString());
        }

    }
}

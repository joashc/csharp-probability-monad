# C# Probability Monad

In their fantastic paper *[Practical Probabilistic Programming with Monads](http://mlg.eng.cam.ac.uk/pub/pdf/SciGhaGor15.pdf)*, Ścibior, Ghahramani, and Gordon (2015) describe a monadic abstraction over probability distributions that can be used to elegantly construct various Bayesian models and perform inference on them. This library implements the approach described in that paper.


## Finite distributions

*Practical Probabilistic Programming with Monads* builds on the work of [Erwig and Kollmansberger (2006)](https://web.engr.oregonstate.edu/~erwig/papers/PFP_JFP06.pdf). Erwig and Kollmansberger created a probability monad that explicitly represented every possibility in a finite distribution. While this monad had a very simple implementation, it was limited to small, discrete models.

Even so, the monad they described was quite expressive, so we've implemented this approach as well.

We can define distributions and query the probability of a certain event:

```java
using ProbabilityMonad;
using static ProbabilityMonad.Distributions;

var coin = UniformD("heads", "tails");

// Probability of a coin flip being heads
coin.ProbOf(flip => flip == "heads");
// 50%

var die = UniformD(1, 2, 3, 4, 5, 6);

// Probability of a die roll being 1 or 2
die.ProbOf(roll => roll == 1 || roll == 2);
// 33.2%
```

We can model independent events, letting us solve a lot of high school probability problems:

```java
var die = UniformD(1, 2, 3, 4, 5);

var threeRolls =
  from roll1 in die
  from roll2 in die
  from roll3 in die
  select new List<int>() { roll1, roll2, roll3 };

// What is the probability that at least two out of
// three rolls of a five-sided die are odd?
var atLeastTwoOdd = threeRolls.ProbOf(rolls =>
  rolls.Where(r => r % 2 == 1)
       .Count() >= 2);
// 64.8%

// Add up the three rolls
var sumRolls = from rolls in threeRolls
               select rolls.Sum();

// What is the probability that three rolls of a
// six-sided die add up to 8?
var sumTo8 = sumRolls.ProbOf(sum => sum == 8);
// 9.7%
```

Because we have a monadic DSL, we can make 

```java
var die = UniformD(1, 2, 3, 4, 5, 6);

// You roll a die and then flip a coin.
// If the coin comes up heads, you add one to your total.
// If you do this twice, what is the probability that your total is 9?

Func<int, FiniteDist<int>>
add1IfHeads = UniformD(x + 1, x);
var rollAndFlip =
  from roll in die
  from maybeAdded in add1IfHeads(roll)
  select maybeAdded;

var rollFlipTwice =
  from first in rollAndFlip
  from second in rollAndFlip
  select first + second;

var pr9 = rollFlipTwice.ProbOf(sum => sum == 9);
```

### Bayesian networks

The monadic style allows us to build up distributions from smaller distributions that depend on each other in some way. This means we can write Bayesian networks in a very natural way:

```java
// Truth table for grass being wet
Func<bool, bool, Prob> wetProb = (rain, sprinkler) =>
{
    if (rain && sprinkler) return Prob(0.98);
    if (rain && !sprinkler) return Prob(0.8);
    if (!rain && sprinkler) return Prob(0.9);
    return Prob(0);
};

// Bayesian network for sprinkler model
var sprinklerModel =
     from rain in Bernoulli(Prob(0.2))
     from sprinkler in Bernoulli(Prob(rain ? 0.01 : 0.4))
     from wet in Bernoulli(wetProb(rain, sprinkler))
     select new SprinklerEvent(rain, sprinkler, wet);

// Probability it rained should still be 20%
var prRained = sprinklerModel.ProbOf(e => e.Raining);
// 20%

var prRainedAndWet = sprinklerModel.ProbOf(e => e.Raining && e.GrassWet);
// 16%
```

We can now condition on certain events, to answer questions like *What is the probability it rained, given that the grass is wet?*

```java
var givenGrassWet = sprinklerModel.ConditionHard(e => e.GrassWet);

var prRainingGivenWet = givenGrassWet.ProbOf(e => e.Raining);
// 35.7%
```

Doing this results in 35.7%. This must be correct because it's the same answer that Wikipedia gives.

### Monty Hall
We can naturally model the Monty Hall problem, and generalize it to arbitrary numbers of doors.

```java
internal class MontyHallState
{
    public Door Winner { get; }
    public Door Picked { get; }

    public MontyHallState(Door winner, Door picked)
    {
        Winner = winner;
        Picked = picked;
    }
}

// Monty hall without switching
Func<List<Door>, FiniteDist<MontyHallState>> 
montyHall = doors =>
    from winner in EnumUniformD(doors)
    from picked in EnumUniformD(doors)
    select new MontyHallState(winner, picked);
```

Let's see the probability of us winning if we stay:

```java
// You won if you picked the winner 
Func<MontyHallState, bool> won = s => s.Picked == s.Winner;

var threeDoors = new List<Door> {
    new Door("1"),
    new Door("2"),
    new Door("3")
};
var prStayWon = montyHall(threeDoors).ProbOf(won);
// 33.3%
```

We have a one in three chance of winning if we stay with our original choice. What if we switch?

```java
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

var prSwitchWon = switched(threeDoors).ProbOf(won);
// 66.6%
```

Switching gives a two in three chance of winning! We can also generalize to more doors:

```java
// Four door monty hall
var fourDoors = new List<Door> {
    new Door("1"),
    new Door("2"),
    new Door("3"),
    new Door("4")
};

var prStayWon4 = montyHall(fourDoors).ProbOf(won);
// 25%

var prSwitchWon4 = switched(fourDoors).ProbOf(won);
// 37.5%
```

## Continuous distributions
Of course, these are quite trivial examples, and definitely don't scale well. We can model 

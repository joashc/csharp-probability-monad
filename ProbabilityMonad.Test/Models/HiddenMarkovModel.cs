using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ProbabilityMonad.Base;

namespace ProbabilityMonad.Test.Models
{
    public static class HiddenMarkovModel
    {
        /// <summary>
        /// Generates data for the hidden markov model
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="emissionDist"></param>
        /// <param name="startDist"></param>
        /// <param name="transitionDist"></param>
        /// <param name="numSamples"></param>
        /// <returns></returns>
        public static Dist<List<Tuple<A, B>>> Sample<A, B>(
            Func<A, Dist<B>> emissionDist,
            Dist<List<A>> startDist,
            Func<A, Dist<A>> transitionDist,
            int numSamples
        )
        {
            Dist<List<Tuple<A, B>>> samples =
                from list in startDist
                let initial = list.First()
                from emit in emissionDist(initial)
                select new List<Tuple<A, B>> { Tuple(initial, emit) };

            while (numSamples > 0)
            {
                samples = from prev in samples
                          from next in transitionDist(prev.Last().Item1)
                          from emit in emissionDist(next)
                          select Append(prev, Tuple(next, emit)).ToList();
                numSamples -= 1;
            }
            return samples;
        }

        /// <summary>
        /// Converts a list of ints into a string
        /// </summary>
        /// <param name="latent"></param>
        /// <returns></returns>
        public static string ShowLatentList(List<int> latent)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            foreach (var i in latent)
            {
                sb.Append($"{i}, ");
            }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Sample data set
        /// </summary>
        public static List<double> ObservedHmmData1 = new List<double>
        {
            0.9,0.8,0.7,0,-0.025,5,2,0.1,0,0.13,0.45,6,0.2,0.3,-1,-1
        };

        /// <summary>
        /// An example hidden Markov model
        /// </summary>
        /// <param name="observed"></param>
        /// <returns></returns>
        public static Dist<List<int>> Hmm(List<double> observed)
        {
            // Latent variables are discrete
            var states = new List<int> { -1, 0, 1 };

            Func<List<double>, Dist<int>>
            transitionDist = ps => Primitive(new FiniteDist<int>(
                Samples(states.Zip(ps, (s, p) => new ItemProb<int>(s, Prob(p))))
            ).ToSampleDist());

            var tNeg1 = transitionDist(new List<double> { 0.1, 0.4, 0.5 });
            var t0 = transitionDist(new List<double> { 0.2, 0.6, 0.2 });
            var t1 = transitionDist(new List<double> { 0.15, 0.7, 0.15 });

            Func<int, Dist<int>> transitionMatrix = i =>
            {
                if (i == -1) return tNeg1;
                if (i == 0) return t0;
                if (i == 1) return t1;
                throw new ArgumentException("Invalid state specified");
            };

            var startDist = Primitive(EnumUniformF(states.Select(s => new List<int> { s })).ToSampleDist());

            // Model the observed values as latent variables with gaussian noise
            Func<int, double, Prob>
            emission = (x, y) => Pdf(NormalC(x, 1), y);

            Func<Dist<List<int>>, double, Dist<List<int>>>
            expand = (d, y) =>
                Condition(xs => emission(xs.Last(), y),
                (from rest in d
                 from x in transitionMatrix(rest.Last())
                 select Append(rest, x).ToList()));

            return observed.Aggregate(startDist, expand);
        }
    }
}


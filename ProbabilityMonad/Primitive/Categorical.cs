﻿using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbCSharp
{
    /// <summary>
    /// Primitive Categorical distribution
    /// </summary>
    public class CategoricalPrimitive<T> : PrimitiveDist<T>
    {
        public double[] ProbabilityMass { get; }
        public T[] Items { get; }
        public Random Gen { get; }
        public Categorical categorical { get; }
        public Dictionary<T, int> ItemIndex { get; }

        public CategoricalPrimitive(T[] items, double[] probabilities, Random gen)
        {
            Gen = gen;
            Items = items;
            ProbabilityMass = probabilities;
            categorical = new Categorical(ProbabilityMass, gen);
            ItemIndex = new Dictionary<T, int>();
            for (int c = 0; c < items.Length; c++)
            {
                ItemIndex.Add(items[c], c);
            }
        }

        public Func<T> Sample
            => () => Items[categorical.Sample()];
    }
}

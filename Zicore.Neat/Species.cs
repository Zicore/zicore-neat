using System;
using System.Collections.Generic;
using System.Linq;

namespace Zicore.Neat
{
    public class Species
    {
        public Species(NeatEvaluator evaluator)
        {
            Evaluator = evaluator;
        }

        public float CompatibilityThresholdModifier { get; set; }
        public float LastFitness { get; set; }
        public int StagnationCounter { get; set; }

        public int Rank { get; private set; }
        public void SetRank(int rank)
        {
            Rank = rank;
        }
        public int Offspring { get; set; }

        public Genome Champion
        {
            get { return Genomes.Items.OrderByDescending(x => x.Fitness).FirstOrDefault(); }
        }

        public NeatEvaluator Evaluator { get; }

        public Genome Representative { get; set; }
        public FitnessCollection Genomes { get; } = new FitnessCollection();

        public void Add(Genome genome)
        {
            Genomes.Add(genome);
        }

        public void SelectRepresentative()
        {
            int index = Evaluator.Random.Next(0, Genomes.Count);
            var genome = Genomes.Items[index];
            Representative = genome;
        }

        public float CalcuateAdjustedFitnessSum()
        {
            AdjustedFitnessSum = Genomes.Items.Sum(x => x.Fitness / Genomes.Count);
            if (float.IsNaN(AdjustedFitnessSum))
            {
                throw new InvalidOperationException($"{nameof(AdjustedFitnessSum)} is NaN");
            }
            return AdjustedFitnessSum;
        }

        public float AdjustedFitnessSum { get; private set; }
        public bool Stagnated { get; set; }

        public void ClearFitness()
        {
            AdjustedFitnessSum = 0.0f;
        }

        public List<Genome> Selection()
        {
            float selectionChance = Evaluator.NeatConfig.SelectionProbability;

            List<Genome> selectedGenomes = new List<Genome>();
            int genomesToTake = (int)System.Math.Ceiling(Genomes.Count * selectionChance);
            
            // only take the top half for the next gen
            foreach (var genome in Genomes.Items.OrderByDescending(x=>x.Fitness).Take(genomesToTake))
            {
                selectedGenomes.Add(genome);
            }

            return selectedGenomes;
        }

        //public int GetSkewedIndex(List<int> indices, int n, float slope = -1.0f)
        //{
        //    float inv_l = 1.0f / n * slope;

        //    foreach (var index in indices)
        //    {
        //        var x = Mathf.Exp(-index);
        //    }

        //    return 0;
        //}
    }
}

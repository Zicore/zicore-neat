using System;
using System.Collections.Generic;
using System.Linq;

namespace Zicore.Neat
{
    public class FitnessCollection
    {
        public FitnessCollection(int limit = Int32.MaxValue)
        {
            Limit = limit;
        }

        public List<Genome> Items { get; } = new List<Genome>();
        public int Limit { get; set; }

        public void Add(Genome genome)
        {
            //if (Items.Count < Limit)
            //{
                Items.Add(genome);
            //}
            //else
            //{
            //    var unfitest = Items.OrderBy(x => x.Fitness).First();
            //    if (genome.Fitness > unfitest.Fitness)
            //    {
            //        // when the new is better than the unfitest, remove the unfitest and add the new
            //        Items.Remove(unfitest);
            //        Items.Add(genome);
            //    }
            //}
        }

        public string FitestDebugText()
        {
            return "{" + string.Join(" | ", Items.OrderByDescending(x => x.Fitness).Select(x => $"{x.GenomeId}: {x.Fitness:0.0}")) + "}";
        }

        public int Count => Items.Count;

        public void Clear()
        {
            Items.Clear();
        }
    }
}

using System.Collections.Generic;

namespace Zicore.Neat
{
    public class GeneComparsionResult
    {
        public GeneComparsionResult(Genome genome1, Genome genome2)
        {
            Genome1 = genome1;
            Genome2 = genome2;
        }

        public Genome Genome1 { get; set; }
        public Genome Genome2 { get; set; }

        public List<(Genome genome1, ConnectionGene connection1, Genome genome2, ConnectionGene connection2)> MatchingGenes { get; set; } = new List<(Genome genome1, ConnectionGene connection1, Genome genome2, ConnectionGene connection2)>();
        public List<(Genome genome, ConnectionGene connection)> DisjointGenes { get; set; } = new List<(Genome genome, ConnectionGene connection)>();
        public List<(Genome genome, ConnectionGene connection)> ExcessGenes { get; set; } = new List<(Genome genome, ConnectionGene connection)>();

        public float WeightDifference { get; set; }
    }
}

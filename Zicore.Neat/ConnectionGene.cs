using Zicore.Neat.Base;

namespace Zicore.Neat
{
    public class ConnectionGene : IConnectionGene
    {
        public Genome Genome { get; set; }

        public int Input { get; set; }
        public int Output { get; set; }
        public float Weight { get; set; }
        public int InnovationNumber { get; set; }
        public bool Enabled { get; set; }

        public NodeGene InputNode
        {
            get
            {
                Genome.NodeCollection.Nodes.TryGetValue(Input, out var gene);
                return gene;
            }
        }

        public NodeGene OutputNode
        {
            get
            {
                Genome.NodeCollection.Nodes.TryGetValue(Output, out var gene);
                return gene;
            }
        }

        public static ConnectionGene Copy(ConnectionGene connectionGene, Genome genome)
        {
            return new ConnectionGene
            {
                InnovationNumber = connectionGene.InnovationNumber,
                Enabled = connectionGene.Enabled,
                Output = connectionGene.Output,
                Input = connectionGene.Input,
                Weight = connectionGene.Weight,
                Genome = genome
            };
        }

        public ConnectionGene Copy(Genome genome)
        {
            return Copy(this, genome);
        }
        
        public override string ToString()
        {
            return $"ID: {InnovationNumber}, {nameof(Input)}: {Input}, {nameof(Output)}: {Output}, {nameof(Weight)}: {Weight}";
        }
    }
}

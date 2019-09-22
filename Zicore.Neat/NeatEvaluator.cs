using System;
using System.Collections.Generic;
using System.Linq;

namespace Zicore.Neat
{
    public class NeatEvaluator
    {
        public NeatEvaluator()
        {
            SpeciesCollection = new SpeciesCollection(this);
        }
        
        public List<NodeGene> InitialNodes { get; } = new List<NodeGene>();

        public int Generation { get; private set; } = 1;

        public int Population { get; set; } = 80;
        public int Inputs { get; set; }
        public int Outputs { get; set; }

        public SpeciesCollection SpeciesCollection { get; private set; }

        public Dictionary<Genome, Species> LastGenerationWithSpecies { get; private set; } = new Dictionary<Genome, Species>();
        public List<Genome> LastGeneration { get; } = new List<Genome>();
        public List<Genome> CurrentGeneration { get; } = new List<Genome>();
        public int LastFillUp { get; set; }

        public Random Random { get; set; }

        public int InnovationId { get; private set; }
        public int NodeId { get; private set; }
        public int GenomeId { get; private set; }

        public ConnectionGeneCollection Connections { get; } = new ConnectionGeneCollection();
        public NeatConfig NeatConfig { get; set; } = new NeatConfig();

        public void Populate()
        {
            Populate(Population);
        }

        public void Populate(int population)
        {
            for (var i = 0; i < population; i++)
            {
                var genome = Genome.CreateDefault(this);
                CurrentGeneration.Add(genome);
            }
        }

        public void NextGeneration()
        {
            SpeciesCollection.ClearSpecies();
            foreach (var genome in CurrentGeneration)
            {
                SpeciesCollection.Speciate(genome);
            }
            SpeciesCollection.ClearEmptySpecies();

            int population = Population;

            var nextGeneration = SpeciesCollection.Selection(population);
            LastGenerationWithSpecies = nextGeneration;

            // fill up when there is no species or
            var remaining = population - nextGeneration.Count;
            for (int i = 0; i < remaining; i++)
            {
                nextGeneration.Add(Genome.CreateDefault(this), null);
            }

            //Populate(population);

            LastFillUp = remaining;

            LastGeneration.Clear();
            LastGeneration.AddRange(CurrentGeneration);
            CurrentGeneration.Clear();
            CurrentGeneration.AddRange(nextGeneration.Select(x => x.Key));

            Generation++;
        }

        public void Reset(int population, int inputs, int outputs)
        {
            Generation = 0;
            InnovationId = 0;
            GenomeId = 0;
            NodeId = 0;
            InitialNodes.Clear();
            CurrentGeneration.Clear();
            LastGeneration.Clear();
            SpeciesCollection.SpeciesItems.Clear();
            Connections.Clear();
            Initialize(population, inputs, outputs);
        }
        
        public void Initialize(int population, int inputs, int outputs)
        {
            Population = population;
            Inputs = inputs;
            Outputs = outputs;

            Random = new Random();
            InitialNodes.AddRange(CreateInitNodes());
        }

        private IEnumerable<NodeGene> CreateInitNodes()
        {
            NodeGene node;
            for (var i = 0; i < Inputs; i++)
            {
                node = new NodeGene {Id = GetNextNodeId(), Type = NodeGeneType.Sensor};
                yield return node;
            }

            for (var i = 0; i < Outputs; i++)
            {
                node = new NodeGene {Id = GetNextNodeId(), Type = NodeGeneType.Output};
                yield return node;
            }
        }

        public NodeGene CreateHiddenNode(int nodeId)
        {
            var node = new NodeGene
            {
                Id = nodeId,
                Type = NodeGeneType.Hidden
            };
            return node;
        }

        public ConnectionGene CreateConnection(Genome genome,int innovationId,int input, int output, float weight)
        {
            var connection = new ConnectionGene
            {
                Enabled = true,
                InnovationNumber = innovationId,
                Input = input,
                Output = output,
                Weight = weight,
                Genome = genome
            };

            return connection;
        }

        public bool AddConnection(ConnectionGene connection)
        {
            if (Connections.AddNew(connection.Copy(null)))
            {
                //IncrementInnovationId();
                return true;
            }

            return false;
        }

        public float GetRandomNumber(float minimum, float maximum)
        {
            return (float) Random.NextDouble() * (maximum - minimum) + minimum;
        }

        public float GetRandomFloat()
        {
            return (float) Random.NextDouble();
        }

        public int GetNextNodeId()
        {
            return ++NodeId;
        }

        public int GetNextInnovationId()
        {
            return ++InnovationId;
        }

        public int GetNextGenomeId()
        {
            return ++GenomeId;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Zicore.Neat.Base;

namespace Zicore.Neat
{
    public class Genome : IGenome
    {
        public Genome(NeatEvaluator evaluator)
        {
            Evaluator = evaluator;
        }

        public static Genome CreateDefault(NeatEvaluator evaluator)
        {
            var g = new Genome(evaluator);
            g.GenomeId = evaluator.GetNextGenomeId();
            g.InitializeDefaultNodes();
            return g;
        }

        public static Genome CreateFromCrossover(NeatEvaluator evaluator, IEnumerable<NodeGene> nodes,IEnumerable<ConnectionGene> connections)
        {
            var g = new Genome(evaluator);
            g.GenomeId = evaluator.GetNextGenomeId();
            g.InitializeDefaultNodes();
            g.InitializeFromCrossover(nodes,connections);
            return g;
        }

        private void InitializeDefaultNodes()
        {
            GenomeId = Evaluator.GetNextGenomeId();
            var nodes = Evaluator.InitialNodes;
            foreach (var node in nodes)
            {
                NodeCollection.AddNew(node);
            }
        }

        public void InitializeFromCrossover(IEnumerable<NodeGene> nodes, IEnumerable<ConnectionGene> connections)
        {
            //foreach (var nodeGene in nodes)
            //{
            //    NodeCollection.AddNew(nodeGene);
            //}
            foreach (var connectionGene in connections)
            {
                var node1 = new NodeGene
                {
                    Type = NodeGeneType.Hidden,
                    Id = connectionGene.Input,
                };

                var node2 = new NodeGene
                {
                    Type = NodeGeneType.Hidden,
                    Id = connectionGene.Output
                };

                NodeCollection.AddNew(node1);
                NodeCollection.AddNew(node2);
                connectionGene.Genome = this;
                ConnectionCollection.AddNew(connectionGene);
            }

        }

        public float Fitness { get; set; }
        public int GenomeId { get; private set; }

        public IEnumerable<INodeGene> Nodes => NodeCollection.NodeList;
        public IEnumerable<IConnectionGene> Connections => ConnectionCollection.ConnectionList;

        public NeatEvaluator Evaluator { get; }
        public NeatConfig Config => Evaluator.NeatConfig;

        public ConnectionGeneCollection ConnectionCollection { get; } = new ConnectionGeneCollection();
        public NodeGeneCollection NodeCollection { get; } = new NodeGeneCollection();

        public void Mutate()
        {
            var changeGeneWeight = Evaluator.GetRandomFloat();
            if (changeGeneWeight < Config.ChangeGeneWeightProbability)
            {
                var mutateWeightShift = Evaluator.GetRandomFloat();
                if (mutateWeightShift < Config.MutateWeightShiftProbablity)
                {
                    MutateWeightShift();
                }
                else
                {
                    MutateNewWeight();
                }
                //var mutateNewWeight = Observer.GetRandomFloat();
                //if(mutateNewWeight < Config.MutateNewWeightProbablity)
                //{
                //    MutateNewWeight();
                //}
            }
            else
            {
                var mutateConnectionEnabledToggle = Evaluator.GetRandomFloat();
                if (mutateConnectionEnabledToggle < Config.MutateConnectionEnabledProbability)
                {
                    MutateConnectionEnabled();
                }
            }

            var mutateConnection = Evaluator.GetRandomFloat();
            if (mutateConnection < Config.MutateConnectionProbability)
            {
                MutateConnection();
            }

            var mutateNode = Evaluator.GetRandomFloat();
            if (mutateNode < Config.MutateNodeProbability)
            {
                MutateNode();
            }
        }

        public bool AddConnection(int inputKey, int outputKey)
        {
            var input = NodeCollection.Nodes[inputKey];
            var output = NodeCollection.Nodes[outputKey];

            if (NodeCollection.Nodes.Count == 0)
                return false;

            bool geneValid = !Config.FeedForwardNetwork || !((input.Type & output.Type) == NodeGeneType.Sensor || (input.Type & output.Type) == NodeGeneType.Output || input.Id == output.Id);
            
            (int, int) key = (input.Id, output.Id);

            if (geneValid)
            {
                if (ConnectionCollection.Exists(key))
                {
                    return false;
                }

                ConnectionGene connection;
                if (!Evaluator.Connections.Exists(key))
                {
                    connection = new ConnectionGene
                    {
                        Enabled = true,
                        Input = input.Id,
                        Output = output.Id,
                        Weight = Evaluator.GetRandomNumber(-Config.WeightMutationPower, Config.WeightMutationPower),
                        InnovationNumber = Evaluator.GetNextInnovationId(),
                        Genome = this
                    };
                    Evaluator.AddConnection(connection);
                }
                else
                {
                    // old connection gets copied
                    connection = Evaluator.Connections.Get(key).Copy(this);
                    connection.Weight = Evaluator.GetRandomNumber(-Config.WeightMutationPower,
                        Config.WeightMutationPower);

                    if (connection.Input != key.Item1 || connection.Output != key.Item2)
                        throw new InvalidOperationException();

                    connection.Enabled = true;
                }

                ConnectionCollection.AddNew(connection);
                return true;
            }
            return false;
        }

        public void MutateConnection()
        {
            bool geneAdded = false;
            while (!geneAdded)
            {
                if (NodeCollection.Nodes.Count == 0)
                    break;

                bool geneValid = true;

                int indexInput = Evaluator.Random.Next(0, NodeCollection.Count);
                int indexOutput = Evaluator.Random.Next(0, NodeCollection.Count);
                var input = NodeCollection[indexInput];
                var output = NodeCollection[indexOutput];

                //if (input.Id >= output.Id || (input.Type & output.Type) == NodeGeneType.Sensor)
                //    geneValid = false;

                if (Config.FeedForwardNetwork && ((input.Type & output.Type) == NodeGeneType.Sensor ||
                                                  (input.Type & output.Type) == NodeGeneType.Output ||
                                                  input.Id == output.Id) ||
                    //(output.Type == NodeGeneType.Output && input.Type == NodeGeneType.Hidden) ||
                    //(input.Type == NodeGeneType.Output && output.Type == NodeGeneType.Sensor) ||
                    input.Type == NodeGeneType.Output || output.Type == NodeGeneType.Sensor)
                {
                    geneValid = false;
                }

                (int, int) key = (input.Id, output.Id);

                if (geneValid)
                {
                    if (ConnectionCollection.Exists(key))
                    {
                        geneValid = false;
                        break;
                    }

                    ConnectionGene connection;
                    if (!Evaluator.Connections.Exists(key))
                    {
                        connection = new ConnectionGene
                        {
                            Enabled = true,
                            Input = input.Id,
                            Output = output.Id,
                            Weight = Evaluator.GetRandomNumber(-Config.WeightMutationPower, Config.WeightMutationPower),
                            InnovationNumber = Evaluator.GetNextInnovationId(),
                            Genome = this
                        };
                        Evaluator.AddConnection(connection);
                    }
                    else
                    {
                        // old connection gets copied
                        connection = Evaluator.Connections.Get(key).Copy(this);
                        connection.Weight = Evaluator.GetRandomNumber(-Config.WeightMutationPower,
                            Config.WeightMutationPower);

                        if (connection.Input != key.Item1 || connection.Output != key.Item2)
                            throw new InvalidOperationException();

                        connection.Enabled = true;
                    }

                    geneAdded = true;
                    ConnectionCollection.AddNew(connection);
                }
            }
        }

        // Makes crossover unstable
        public void MutateNode_RearrangeNetworkTest()
        {
            if (ConnectionCollection.Count <= 0)
                return;

            var connection = GetRandomConnection();
            connection.Enabled = false;
            DebugConnection(connection);
            
            var newNode = Evaluator.CreateHiddenNode(connection.Output);
            var oldNode = NodeCollection.Nodes[connection.Output];
            NodeCollection.Nodes.Remove(oldNode.Id);
            oldNode.Id = Evaluator.GetNextNodeId();
            NodeCollection.Nodes[newNode.Id] = newNode;
            NodeCollection.Nodes[oldNode.Id] = oldNode;

            // swap old connection to shifted node
            (int, int) oldCon = (connection.Input, connection.Output);
            connection.Output = newNode.Id;
            ConnectionCollection.UpdateInOut(oldCon, connection);

            foreach (var con in ConnectionCollection.Connections.Where(x => x.Value.Input == newNode.Id || x.Value.Output == newNode.Id).ToList())
            {
                (int, int) oldKey = (con.Value.Input, con.Value.Output);
                if (con.Value.Input == newNode.Id)
                {
                    con.Value.Input = oldNode.Id;
                }

                if (con.Value.Output == newNode.Id)
                {
                    con.Value.Output = oldNode.Id;
                }

                ConnectionCollection.UpdateInOut(oldKey, con.Value);
            }

            var con1 = Evaluator.CreateConnection(this, Evaluator.GetNextInnovationId(), connection.Input, newNode.Id, Config.MutateNodeWeightInitialValue);
            var con2 = Evaluator.CreateConnection(this, Evaluator.GetNextInnovationId(), newNode.Id, oldNode.Id, connection.Weight);

            bool addedCon1 = Evaluator.AddConnection(con1);
            bool addedCon2 = Evaluator.AddConnection(con2);
            bool addedconnection = Evaluator.AddConnection(connection);

            bool addedConLocal1 = ConnectionCollection.AddNew(con1);
            bool addedConLocal2 = ConnectionCollection.AddNew(con2);

            NodeCollection.Nodes.Remove(newNode.Id);
            NodeCollection.AddNew(oldNode);
            NodeCollection.AddNew(newNode);
        }

        public void MutateNode()
        {
            if (ConnectionCollection.Count <= 0)
                return;

            var connection = GetRandomConnection();
            connection.Enabled = false;
            DebugConnection(connection);

            var node = Evaluator.CreateHiddenNode(Evaluator.GetNextNodeId());

            var con1 = Evaluator.CreateConnection(this, Evaluator.GetNextInnovationId(), connection.Input, node.Id, Config.MutateNodeWeightInitialValue);
            var con2 = Evaluator.CreateConnection(this, Evaluator.GetNextInnovationId(), node.Id, connection.Output, connection.Weight);
           
            bool addedCon1 = Evaluator.AddConnection(con1);
            bool addedCon2 = Evaluator.AddConnection(con2);

            bool addedConLocal1 = ConnectionCollection.AddNew(con1);
            bool addedConLocal2 = ConnectionCollection.AddNew(con2);

            bool nodeAdded = NodeCollection.AddNew(node);
            if (!nodeAdded)
                throw new InvalidOperationException();


            if (NodeCollection.Get(con1.Input) == null)
                throw new InvalidOperationException();
        }

        private void DebugConnection(ConnectionGene c)
        {

        }

        public void MutateConnectionEnabled()
        {
            if (ConnectionCollection.Count <= 0)
                return;

            var connection = GetRandomConnection();
            connection.Enabled = !connection.Enabled;
        }

        public void MutateWeightShift()
        {
            if (ConnectionCollection.Count <= 0)
                return;

            var connection = GetRandomConnection();
            connection.Weight = connection.Weight *
                                Evaluator.GetRandomNumber(-Config.MutateWeightShiftRange, Config.MutateWeightShiftRange);
            if (Config.UseWeightCap)
            {
                connection.Weight = NeatMath.Clamp(connection.Weight, -Config.WeightCap, Config.WeightCap);
            }
        }

        public void MutateNewWeight()
        {
            if (ConnectionCollection.Count <= 0)
                return;

            var connection = GetRandomConnection();
            connection.Weight = Evaluator.GetRandomNumber(-Config.MutateNewWeightRange, Config.MutateNewWeightRange);
        }

        public ConnectionGene GetRandomConnection()
        {
            var connectionIndex = Evaluator.Random.Next(0, ConnectionCollection.Count);
            var connection = ConnectionCollection[connectionIndex];
            if (connection == null)
                throw new InvalidOperationException();
            return connection;
        }

        public Genome Copy()
        {
            var genome = Genome.CreateDefault(Evaluator);
            genome.Fitness = Fitness;
            foreach (var connectionsConnection in ConnectionCollection.Connections)
            {
                genome.ConnectionCollection.AddNew(connectionsConnection.Value.Copy(this));
            }

            foreach (var node in NodeCollection.Nodes.Values)
            {
                genome.NodeCollection.AddNew(node.Copy());
            }

            return genome;
        }

        public void UpdateSensors(params float[] sensorValues)
        {
            int i = 0;
            foreach (var node in NodeCollection.GetInputs())
            {
                node.Value = sensorValues[i];
                i++;
            }
        }

        public static Dictionary<int, int> GetConnectionKeys(Genome genome1, Genome genome2)
        {
            Dictionary<int, int> keys = new Dictionary<int, int>();
            foreach (var conKey in genome1.ConnectionCollection.Connections.Keys)
            {
                keys[conKey] = conKey;
            }
            foreach (var conKey in genome2.ConnectionCollection.Connections.Keys)
            {
                keys[conKey] = conKey;
            }
            return keys;
        }

        public static GeneComparsionResult CompareGenomes(Genome genome1, Genome genome2)
        {
            int maxCon1 = genome1.ConnectionCollection.MaxInnovationId;
            int maxCon2 = genome2.ConnectionCollection.MaxInnovationId;

            var gen1Set = genome1.ConnectionCollection.Connections;
            var gen2Set = genome2.ConnectionCollection.Connections;

            var allConnectionKeys = GetConnectionKeys(genome1, genome2);

            GeneComparsionResult result = new GeneComparsionResult(genome1, genome2);

            foreach (var key in allConnectionKeys)
            {
                var i = key.Key;
                gen1Set.TryGetValue(key.Key, out var c1);
                gen2Set.TryGetValue(key.Key, out var c2);

                //if(c1 == null && c2 == null)
                //    continue;

                if (c1 != null && c2 != null)
                {
                    result.MatchingGenes.Add((genome1, c1, genome2, c2));
                    var con1 = c1;
                    var con2 = c2;
                    result.WeightDifference += System.Math.Abs(con1.Weight - con2.Weight);
                }
                else if (c1 == null && i < maxCon1)
                    result.DisjointGenes.Add((genome2, c2));
                else if (c1 != null && i < maxCon2)
                    result.DisjointGenes.Add((genome1, c1));
                else if (c1 == null && i > maxCon1)
                    result.ExcessGenes.Add((genome2, c2));
                else if (c1 != null && i > maxCon2)
                    result.ExcessGenes.Add((genome1, c1));
            }

            return result;
        }

        public static float CalculateCompatibilityDistance(GeneComparsionResult result, float c1, float c2, float c3)
        {
            int matching = result.MatchingGenes.Count;
            int disjoint = result.DisjointGenes.Count;
            int excess = result.ExcessGenes.Count;

            float avgWeightDiff = matching == 0 ? 0 : result.WeightDifference / matching;

            return CalculateCompatibilityDistance(c1, c2, c3, 1, avgWeightDiff, disjoint, excess);
        }

        public static float CalculateCompatibilityDistance(float c1, float c2, float c3, int N, float avgWeightDiff, int disjointGenes, int excessGenes)
        {
            float compDistance = (excessGenes * c1 / N) + (disjointGenes * c2 / N) + c3 * avgWeightDiff;
            return compDistance;
        }

        public static Genome Crossover(NeatEvaluator evaluator, Genome genome1, Genome genome2)
        {
            float disableInheritChance = evaluator.NeatConfig.DisableInheritChance;
            float fitnessEqualTolerance = evaluator.NeatConfig.FitnessEqualTolerance;

            var result = CompareGenomes(genome1, genome2);
            var fitParent = genome1.Fitness > genome2.Fitness ? genome1 : genome2;
            bool fitnessEqual = System.Math.Abs(genome1.Fitness - genome2.Fitness) < fitnessEqualTolerance;

            List<ConnectionGene> crossoverConnections = new List<ConnectionGene>();
            List<NodeGene> nodesToTake = new List<NodeGene>();

            foreach (var con in result.MatchingGenes)
            {
                var geneToTake = GetMatchingGene(evaluator, con.connection1, con.connection2, disableInheritChance);
                
                crossoverConnections.Add(geneToTake);
            }

            foreach (var con in result.DisjointGenes)
            {
                var geneToTake = GetGeneFromFitterParent(evaluator, fitParent, fitnessEqual, con.connection, con.genome);
                if (geneToTake != null)
                {
                    crossoverConnections.Add(geneToTake);
                }
            }

            foreach (var con in result.ExcessGenes)
            {
                var geneToTake = GetGeneFromFitterParent(evaluator, fitParent, fitnessEqual, con.connection, con.genome);
                if (geneToTake != null)
                {
                    crossoverConnections.Add(geneToTake);
                }
            }

            nodesToTake.AddRange(genome1.NodeCollection.NodeList.Where(x=>x.Type != NodeGeneType.Hidden));

            return Genome.CreateFromCrossover(evaluator, nodesToTake,crossoverConnections);
        }

        private static ConnectionGene GetMatchingGene(NeatEvaluator evaluator, ConnectionGene connection1, ConnectionGene connection2, float disableInheritChance)
        {
            ConnectionGene geneToTake;
            if (evaluator.GetRandomFloat() < 0.5)
                geneToTake = connection1.Copy(null);
            else
                geneToTake = connection2.Copy(null);

            if (connection1.Enabled || connection2.Enabled)
            {
                if (!connection1.Enabled || !connection2.Enabled)
                {
                    geneToTake.Enabled = true;
                    if (evaluator.GetRandomFloat() < disableInheritChance)
                    {
                        geneToTake.Enabled = false;
                    }
                }
            }

            return geneToTake;
        }

        private static ConnectionGene GetGeneFromFitterParent(NeatEvaluator evaluator, Genome fitParent, bool fitnessEqual, ConnectionGene connection, Genome genome)
        {
            ConnectionGene geneToTake = null;
            if (fitnessEqual)
            {
                if (evaluator.GetRandomFloat() < 0.5)
                {
                    geneToTake = connection.Copy(null);
                }
            }
            else
            {
                if (genome == fitParent)
                {
                    geneToTake = connection.Copy(null);
                }
            }

            return geneToTake;
        }

        public List<float> GetOutputValues()
        {
            List<float> values = new List<float>();
            foreach (var nodeGene in NodeCollection.GetOutputs())
            {
                values.Add(nodeGene.Value);
            }

            return values;
        }

        public string DebugConnections()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in ConnectionCollection.Connections.Values)
            {
                var inputNode = NodeCollection.Get(c.Input);
                var outputNode = NodeCollection.Get(c.Output);
                sb.AppendLine($"Input: {c.Input} (InputNode: {inputNode?.Id}), Output: {c.Output} (OutputNode: {outputNode?.Id}), Weight: {c.Weight}, Enabled: {c.Enabled}");
            }

            var msg = sb.ToString();
            return msg;
        }

        public string DebugValues()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var node in NodeCollection.Nodes.Values)
            {
                sb.AppendLine(
                    $"Node: {node.Id}, Calculated: {node.Evaluated}, Value: {node.Value}, Type: {node.Type}");
            }
            var msg = sb.ToString();
            return msg;
        }

        public void ResetEvauluation()
        {
            NodeCollection.ResetEvaluation();
        }

        public void Evaluate()
        {
            var outputs = NodeCollection.GetOutputs().ToList();
            foreach (var node in outputs)
            {
                CalculateGenome(node);
            }

            if (Evaluator.NeatConfig.DebugMode)
            {
                DebugConnections();
                DebugValues();
            }
        }

        public IEnumerable<ConnectionGene> GetConnections(int nodeId)
        {
            //Connections.GetConnectionByOutputId()
            return ConnectionCollection.Connections.Where(x => x.Value.Output == nodeId).Select(x => x.Value); // replace by dictionary
        }

        public void CalculateGenome(NodeGene node)
        {
            if (!node.Evaluated && node.Type != NodeGeneType.Sensor)
            {
                node.Evaluated = true;
                var connections = GetConnections(node.Id).ToList();
                double sum = 0;
                foreach (var connection in connections)
                {
                    if (connection.Enabled)
                    {
                        var input = NodeCollection.Get(connection.Input);
                        if (!input.Evaluated)
                        {
                            CalculateGenome(input);
                        }
                        sum += input.Value * connection.Weight;
                        //node.Calculate(Observer.NeatConfig.GetActivation(), input.Value, connection.Weight);
                    }
                }

                node.Value = (float)Evaluator.NeatConfig.GetActivation().Invoke(sum);
            }
        }

        public override string ToString()
        {
            return $"Fit:{Fitness:0.00} Links/Active:{ConnectionCollection.Count:000}/{ConnectionCollection.EnabledConnectionCount:000} Nodes: {NodeCollection.Count:000}";
        }

        // Output = Output + Input * Weight
    }
}

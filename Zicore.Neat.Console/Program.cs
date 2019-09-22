using System;
using System.Collections.Generic;
using System.Linq;
using Zicore.Neat.IO;

namespace Zicore.Neat.Console
{
    class Program
    {
        public static void Shuffle<T>(Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        static void Main(string[] args)
        {
            NeatConfig config = new NeatConfig
            {
                DebugMode = false,
                FeedForwardNetwork = true,
                StartWithRandomConnectionMutation = true,
                ActivationFunction = ActivationFunction.Tanh,
                MutateConnectionEnabledProbability = 0.05f,

                ChangeGeneWeightProbability = 0.8f,
                MutateWeightShiftProbablity = 0.90f,
                MutateNewWeightProbablity = 0.10f,

                MutateConnectionProbability = 0.05f,
                MutateNodeProbability = 0.03f,
                WeightMutationPower = 3.2f,
                MutateNodeWeightInitialValue = 1.0f,
                MutateWeightShiftRange = 2f,
                MutateNewWeightRange = 2.0f,
                DisableInheritChance = 0.75f,

                SelectionProbability = 0.5f,

                FitnessEqualTolerance = 0.001f,
                CrossoverOffspringRate = 0.75f,
                InterspeciesMatingRate = 0.001f,
                ChampCopyThreshold = 5,

                DisjointCoefficientC1 = 1f,
                ExcessCoefficientC2 = 1f,
                WeightCoefficientC3 = 1f,
                CompatibilityThresholdDeltaT = 6f,

                UseStagnation = true,
                StagnationThreshold = 15,

                UseWeightCap = false,
                WeightCap = 8,

                UseSpeciesControl = true,
                SpeciesControlRate = 0.3f,
                SpeciesControlTarget = 30
            };

            Exporter exporter = new Exporter();
            //string jsonResult;
            string consoleInput = null;
            do
            {

                NeatEvaluator neat = new NeatEvaluator { NeatConfig = config };
                //neat.Random = new Random(1000);
                neat.Initialize(150, 3, 1);
                neat.Populate();

                // bias and xor input
                float[] inputA = {
                0.0f,
                0.0f,
            };
                float outA = 0.0f;

                float[] inputB = {
                0.0f,
                1.0f,
            };
                float outB = 1.0f;


                float[] inputC = {
                1.0f,
                0.0f,
            };
                float outC = 1.0f;

                float[] inputD = {
                1.0f,
                1.0f,
            };
                float outD = 0.0f;

                var inputs = new List<(float[] xi, float xo)>()
            {
                (inputA, outA),
                (inputB, outB),
                (inputC, outC),
                (inputD, outD),
            }.ToArray();

                Shuffle(new Random(), inputs);

                List<float> input = new List<float>();

                float maxFitness = 0;
                List<(float value, float target)> bestouts = new List<(float value, float target)>();
                bool done = false;
                Genome lastWinning = null;
                while (neat.Generation < 400)
                {
                    foreach (var genome in neat.CurrentGeneration)
                    {
                        List<(float value, float target)> outs = new List<(float value, float target)>();
                        float error = 0;
                        foreach (var (xi, xo) in inputs)
                        {
                            input.Clear();
                            input.Add(1.0f);
                            input.AddRange(xi);

                            genome.ResetEvauluation();
                            genome.UpdateSensors(input.ToArray());
                            genome.Evaluate();
                            var values = genome.GetOutputValues();
                            var outputValue = values[0];
                            var f = xo - outputValue;
                            error += f * f;
                            outs.Add((outputValue, xo));
                        }

                        genome.Fitness = (float)4.0f - error;

                        if (genome.Fitness < 0)
                        {
                            genome.Fitness = 0;
                        }

                        if (genome.Fitness > maxFitness)
                        {
                            maxFitness = Math.Max(genome.Fitness, maxFitness);
                            bestouts.Clear();
                            bestouts.AddRange(outs);
                        }

                        if (IsSolutionCorrect(outs))
                        {
                            lastWinning = genome;
                            done = true;
                            break;
                        }
                    }

                    //Console.WriteLine("Species Ranking:");
                    foreach (var species in neat.SpeciesCollection.SpeciesItems.OrderByDescending(x => x.AdjustedFitnessSum).Take(1))
                    {
                        //var best = species.Genomes.Items.OrderByDescending(x => x.Fitness).FirstOrDefault();
                        //if (best != null)
                        //{
                        //    Console.WriteLine($"Rank:{species.Rank} Gen:{neat.Generation:0000} Pop:{neat.CurrentGeneration.Count} Species:{neat.SpeciesCollection.SpeciesItems.Count} Inno/Nodes:{neat.InnovationId:0000}/{neat.NodeId:0000} LastFill:{neat.LastFillUp} Best:{best} Winner:{lastWinning}");
                        //}

                        //if (species.Champion != null)
                        //{
                        System.Console.WriteLine($"Rank:{species.Rank:00} Gen/Pop:{neat.Generation:0000}/{neat.CurrentGeneration.Count} Species/Genomes:{neat.SpeciesCollection.SpeciesItems.Count:000}/{species.Genomes.Count:000} Inno/Nodes:{neat.InnovationId:0000}/{neat.NodeId:0000} LastFill:{neat.LastFillUp} Best:{species.Champion}");
                        //}
                    }

                    if (done)
                    {
                        break;
                    }

                    neat.NextGeneration();
                }

                if (done)
                {
                    System.Console.WriteLine($"Winner:{lastWinning}");
                }

                System.Console.WriteLine($"Simulation ended! Max Fitness: {maxFitness}");
                System.Console.WriteLine("Press enter to restart xor!");
                consoleInput = System.Console.ReadLine();

            } while (consoleInput != "exit" || consoleInput != "quit");
        }

        public static bool IsSolutionCorrect(IEnumerable<(float value, float target)> values)
        {
            foreach (var valueTuple in values)
            {
                if (!IsValue(valueTuple.value, valueTuple.target))
                    return false;
            }
            return true;
        }

        public static bool IsValue(float value, float target)
        {
            if (target >= 0.5)
                return Is1(value);
            else
            {
                return Is0(value);
            }
        }

        public static bool Is0(float value)
        {
            return value >= 0 && value < 0.5;
        }

        public static bool Is1(float value)
        {
            return value <= 1 && value >= 0.5;
        }
    }
}

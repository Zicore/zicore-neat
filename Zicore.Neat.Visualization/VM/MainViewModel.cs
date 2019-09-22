using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Zicore.Neat.Base;
using Zicore.Neat.IO;
using Zicore.Neat.Visualization.Drawing;

namespace Zicore.Neat.Visualization.VM
{
    public enum MutationOption
    {
        Reset,
        Mutate,
        MutateLink,
        MutateNode,
        MutateWeightShift,
        MutateNewWeight
    }

    public class MainViewModel : ViewModelBase
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
            WeightMutationPower = 2.0f,
            MutateNodeWeightInitialValue = 1.0f,
            MutateWeightShiftRange = 2f,
            MutateNewWeightRange = 2.0f,
            DisableInheritChance = 0.75f,

            SelectionProbability = 0.5f,

            FitnessEqualTolerance = 0.001f,
            CrossoverOffspringRate = 0.75f,
            InterspeciesMatingRate = 0.001f,
            ChampCopyThreshold = 5,

            DisjointCoefficientC1 = 0.5f,
            ExcessCoefficientC2 = 0.5f,
            WeightCoefficientC3 = 0.6f,
            CompatibilityThresholdDeltaT = 3f,

            UseStagnation = true,
            StagnationThreshold = 15,

            UseWeightCap = true,
            WeightCap = 8,

            UseSpeciesControl = true,
            SpeciesControlRate = 0.3f,
            SpeciesControlTarget = 30
        };

        private ICommand startCommand;
        private ICommand testCommand;
        private string statusText;
        private string resultText;

        public ICommand StartCommand => startCommand ?? (startCommand = new RelayCommand(StartNeat));
        public ICommand MutateCommand => testCommand ?? (testCommand = new RelayCommand<MutationOption>(NextGen));

        public RendererVM RendererVM { get; set; } = new RendererVM();

        public string StatusText
        {
            get => statusText;
            set
            {
                if (value == statusText) return;
                statusText = value;
                OnPropertyChanged();
            }
        }

        public string ResultText
        {
            get => resultText;
            set
            {
                if (value == resultText) return;
                resultText = value;
                OnPropertyChanged();
            }
        }

        public static Dispatcher UIDispatcher;

        private void Test()
        {
            UIDispatcher = Dispatcher.CurrentDispatcher;
            RendererVM.Genome = NetworkRenderer.LoadGenome();
        }

        private List<(float[] xi, float xo)> GetInputs()
        {
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

            List<(float[] xi, float xo)> inputs = new List<(float[] xi, float xo)>()
            {
                (inputA, outA),
                (inputB, outB),
                (inputC, outC),
                (inputD, outD),
            };
            return inputs;
        }

        private Genome genomeGen;

        void Prepareneat()
        {
            UIDispatcher = Dispatcher.CurrentDispatcher;
            NeatEvaluator neat = new NeatEvaluator();
            neat.NeatConfig = config;
            neat.Initialize(1, 3, 1);
            neat.Populate();
            genomeGen = neat.CurrentGeneration.FirstOrDefault();
            StatusText = $" Gen/Pop:{neat.Generation:0000}/{neat.CurrentGeneration.Count} Inno/Nodes:{neat.InnovationId:0000}/{neat.NodeId:0000} Genome:{genomeGen}";
        }

        void NextGen(MutationOption option)
        {
            if (genomeGen == null || option == MutationOption.Reset)
            {
                Prepareneat();
            }

            if (option == MutationOption.Mutate)
            {
                genomeGen.Mutate();
            }

            if (option == MutationOption.MutateLink)
            {
                genomeGen.MutateConnection();
            }

            if (option == MutationOption.MutateNode)
            {
                genomeGen.MutateNode();
            }

            if (option == MutationOption.MutateWeightShift)
            {
                genomeGen.MutateWeightShift();
            }

            if (option == MutationOption.MutateNewWeight)
            {
                genomeGen.MutateNewWeight();
            }

            RendererVM.Genome = genomeGen;
            RendererVM.UpdateGenome();
        }

        void StartNeat()
        {
            UIDispatcher = Dispatcher.CurrentDispatcher;
            Task task = new Task(() =>
            {
                ResultText = "";
                DateTime lastTime = DateTime.Now;
                NeatEvaluator neat = new NeatEvaluator { NeatConfig = config };
                //neat.Random = new Random(1000);
                neat.Initialize(150, 3, 1);
                neat.Populate();
                
                List<float> input = new List<float>();

                float maxFitness = 0;
                bool done = false;
                Genome lastWinning = null;

                var inputs = GetInputs().ToArray();
                Shuffle(new Random(), inputs);

                while (neat.Generation < 400)
                {
                    //Console.WriteLine($"Species: {neat.SpeciesCollection.SpeciesItems.Count}");
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
                            // Console.WriteLine($"Fitness: {genome.Fitness:0.0} XI: {xi[0]}, {xi[1]} => XO: {xo} OUT: {values[0]} HIT: {100 - System.Math.Abs(xo - values[0]) * 100.0f}%");
                        }

                        genome.Fitness = (float)4.0f - error;
                        
                        if (genome.Fitness < 0)
                        {
                            genome.Fitness = 0;
                        }

                        if (genome.Fitness > maxFitness)
                        {
                            maxFitness = Math.Max(genome.Fitness, maxFitness);
                        }

                        if (IsSolutionCorrect(outs))
                        {
                            lastWinning = genome;
                            done = true;
                            break;
                        }
                    }

                    foreach (var species in neat.SpeciesCollection.SpeciesItems.OrderByDescending(x => x.AdjustedFitnessSum).Take(1))
                    {
                        if (species.Champion != null)
                        {
                            RendererVM.Genome = species.Champion;
                            StatusText =
                                $"Rank:{species.Rank:00} Gen/Pop:{neat.Generation:0000}/{neat.CurrentGeneration.Count} Species/Genomes:{neat.SpeciesCollection.SpeciesItems.Count:000}/{species.Genomes.Count:000} Inno/Nodes:{neat.InnovationId:0000}/{neat.NodeId:0000} LastFill:{neat.LastFillUp} Best:{species.Champion}";
                        }
                    }

                    if (done)
                    {
                        break;
                    }

                    neat.NextGeneration();
                }

                ResultText = $"Simulation ended! Max Fitness: {maxFitness} Winner: {lastWinning}";
            });
            task.Start();
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
    }
}

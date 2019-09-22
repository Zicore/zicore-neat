using System;

namespace Zicore.Neat
{
    public enum ActivationFunction
    {
        Custom,
        Sigmoid,
        SigmoidSteep,
        Tanh,
        ReLU
    }

    public class NeatConfig
    {
        // -------------------- General Parameter --------------------
        public bool DebugMode { get; set; }
        public bool StartWithRandomConnectionMutation { get; set; }
        public bool FeedForwardNetwork { get; set; }

        // -------------------- Activation Parameter --------------------
        public ActivationFunction ActivationFunction { get; set; } = ActivationFunction.Tanh;

        // -------------------- Mutation Parameter --------------------
        public float MutateConnectionEnabledProbability { get; set; } = 0.05f; // 0.05f

        public float ChangeGeneWeightProbability { get; set; } = 0.9f; // 0.90f
        public float MutateWeightShiftProbablity { get; set; } = 0.90f; // 0.90f
        public float MutateNewWeightProbablity { get; set; } = 0.10f; // 0.10f

        public float MutateConnectionProbability { get; set; } = 0.05f; // 0.05f
        public float MutateNodeProbability { get; set; } = 0.03f; // 0.03f
        
        public float WeightMutationPower { get; set; } = 2.0f; // 2.0f
        public float MutateNodeWeightInitialValue { get; set; } = 1.0f; // 1.0f
        public float MutateWeightShiftRange { get; set; } = 2.0f; // 2.0f
        public float MutateNewWeightRange { get; set; } = 2.0f; // 2.0f

        // -------------------- Selection Parameter --------------------
        public float SelectionProbability { get; set; } = 0.5f;


        // -------------------- Crossover Parameter --------------------
        public float DisableInheritChance { get; set; } = 0.75f;     // 0.75f
        public float FitnessEqualTolerance { get; set; } = 0.001f;   // 0.001f

        public float CrossoverOffspringRate { get; set; } = 0.75f;   // 0.75f
        public float InterspeciesMatingRate { get; set; } = 0.001f;  // 0.001f

        // -------------------- Selection Parameter --------------------
        public int ChampCopyThreshold { get; set; } = 5; // 5

        // -------------------- Compatibility Distance Parameter --------------------
        public float DisjointCoefficientC1 { get; set; } = 1.0f; // 1.0f
        public float ExcessCoefficientC2 { get; set; } = 1.0f; // 1.0f
        public float WeightCoefficientC3 { get; set; } = 0.4f; // 0.4f
        public float CompatibilityThresholdDeltaT { get; set; } = 0.4f; // 0.4f

        // -------------------- Stagnation --------------------
        public bool UseStagnation { get; set; }
        public int StagnationThreshold { get; set; } = 10;

        // -------------------- Weight Cap --------------------
        public bool UseWeightCap { get; set; }
        public float WeightCap { get; set; } = 8.0f;

        public bool UseSpeciesControl { get; set; }
        public float SpeciesControlRate { get; set; } = 0.3f;
        public int SpeciesControlTarget { get; set; } = 10;

        // -------------------- Helper --------------------

        public Func<double, double> GetActivation(Func<double, double> myFunc = null)
        {
            switch (ActivationFunction)
            {
                case ActivationFunction.Custom:
                    return myFunc;
                case ActivationFunction.Sigmoid:
                    return NeatMath.Sigmoid;
                case ActivationFunction.SigmoidSteep:
                    return NeatMath.SigmoidSteep;
                case ActivationFunction.Tanh:
                    return NeatMath.Tanh;
                case ActivationFunction.ReLU:
                    return NeatMath.ReLU;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ActivationFunction), ActivationFunction, null);
            }
        }
    }
}

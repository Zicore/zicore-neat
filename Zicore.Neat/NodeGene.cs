using System;
using Zicore.Neat.Base;

namespace Zicore.Neat
{
    public class NodeGene : INodeGene
    {
        public NodeGeneType Type { get; set; }
        public float Value { get; set; }
        public int Id { get; set; }
        public bool Evaluated { get; set; }
        
        public static NodeGene Copy(NodeGene node)
        {
            return new NodeGene
            {
                Id = node.Id,
                Type = node.Type,
            };
        }

        public NodeGene Copy()
        {
            return Copy(this);
        }

        public void Calculate(Func<double, double> activation,float input, float weight)
        {
            Value = (float)activation(Value + input * weight);
            //Value = (float)activation(Value + input) * weight;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Type)}: {Type}, {nameof(Value)}: {Value}";
        }

    }
}

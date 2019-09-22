namespace Zicore.Neat.Base
{
    public interface INodeGene
    {
        NodeGeneType Type { get; set; }
        float Value { get; set; }
        int Id { get; set; }
        bool Evaluated { get; set; }
    }
}
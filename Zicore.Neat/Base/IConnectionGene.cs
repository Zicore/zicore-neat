namespace Zicore.Neat.Base
{
    public interface IConnectionGene
    {
        int Input { get; set; }
        int Output { get; set; }
        float Weight { get; set; }
        int InnovationNumber { get; set; }
        bool Enabled { get; set; }
    }
}
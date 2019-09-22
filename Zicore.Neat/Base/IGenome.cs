using System.Collections.Generic;

namespace Zicore.Neat.Base
{
    public interface IGenome
    {
        float Fitness { get; set; }
        int GenomeId { get; }

        IEnumerable<INodeGene> Nodes { get; }
        IEnumerable<IConnectionGene> Connections { get; }
    }
}
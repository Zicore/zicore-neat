using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Zicore.Neat.Base;

namespace Zicore.Neat.IO.Model
{
    [JsonObject("Genome")]
    public class ExportGenome : IGenome
    {
        public float Fitness { get; set; }
        public int GenomeId { get; set; }

        public IEnumerable<IConnectionGene> Connections { get; set; } = new List<IConnectionGene>();
        public IEnumerable<INodeGene> Nodes { get; set; } = new List<INodeGene>();
    }
}

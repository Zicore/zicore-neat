using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Zicore.Neat.Base;

namespace Zicore.Neat.IO.Model
{
    [JsonObject("Node")]
    public class ExportNodeGene : INodeGene
    {
        public NodeGeneType Type { get; set; }
        public float Value { get; set; }
        public int Id { get; set; }
        public bool Evaluated { get; set; }
    }
}

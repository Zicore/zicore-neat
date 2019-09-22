using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Zicore.Neat.Base;

namespace Zicore.Neat.IO.Model
{
    [JsonObject("Gene")]
    public class ExportConnectionGene : IConnectionGene
    {
        public int Input { get; set; }
        public int Output { get; set; }
        public float Weight { get; set; }
        public int InnovationNumber { get; set; }
        public bool Enabled { get; set; }
    }
}

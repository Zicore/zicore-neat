using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zicore.Neat.Base;
using Zicore.Neat.IO.Model;

namespace Zicore.Neat.IO
{
    public class Exporter
    {
        public string Export(Genome genome)
        {
            //ExportGenome exportGenome = new ExportGenome
            //{
            //    Fitness = genome.Fitness,
            //    GenomeId = genome.GenomeId
            //};

            //foreach (var c in genome.ConnectionCollection.ConnectionList.OrderBy(x=>x.Input))
            //{
            //    exportGenome.Connections.Add(new ExportConnectionGene
            //    {
            //        Input = c.Input,
            //        Output = c.Output,
            //        Weight = c.Weight,
            //        InnovationNumber = c.InnovationNumber,
            //        Enabled = c.Enabled
            //    });
            //}

            //foreach (var n in genome.NodeCollection.Nodes.Values)
            //{
            //    exportGenome.Nodes.Add(new ExportNodeGene
            //    {
            //        Id = n.Id,
            //        Value = n.Value,
            //        Type = n.Type,
            //        Calculated = n.Calculated
            //    });
            //}

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new InterfaceContractResolver(typeof(IGenome)),
                Converters = new List<JsonConverter>
                {
                    new ConnectionGeneConverter(), new NodeGeneConverter()
                },
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(genome, settings);
        }

        public static T Import<T>(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new InterfaceContractResolver(typeof(IGenome)),
                Converters = new List<JsonConverter>
                {
                    new ConnectionGeneConverter(), new NodeGeneConverter()
                },
                Formatting = Formatting.Indented
            };

            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public class NodeGeneConverter : CustomCreationConverter<INodeGene>
        {
            public override INodeGene Create(Type objectType)
            {
                return new ExportNodeGene();
            }
        }

        public class ConnectionGeneConverter : CustomCreationConverter<IConnectionGene>
        {
            public override IConnectionGene Create(Type objectType)
            {
                return new ExportConnectionGene();
            }
        }
    }
}

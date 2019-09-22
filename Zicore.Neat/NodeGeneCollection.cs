using System.Collections.Generic;
using System.Linq;

namespace Zicore.Neat
{
    public class NodeGeneCollection
    {
        public Dictionary<int, NodeGene> Nodes { get; } = new Dictionary<int, NodeGene>();
        public List<NodeGene> NodeList { get; } = new List<NodeGene>();

        public bool AddNew(NodeGene node)
        {
            if (!Nodes.ContainsKey(node.Id))
            {
                Nodes[node.Id] = node;
                NodeList.Add(node);
                return true;
            }
            return false;
        }

        public int Count => Nodes.Count;

        public NodeGene this[int index]
        {
            get => NodeList[index];
            set => NodeList[index] = value;
        }

        public NodeGene Get(int key)
        {
            if (Nodes.ContainsKey(key))
            {
                return Nodes[key];
            }

            return null;
        }

        public IEnumerable<NodeGene> GetOutputs()
        {
            return Nodes.Where(x => x.Value.Type == NodeGeneType.Output).Select(x=>x.Value);
        }

        public IEnumerable<NodeGene> GetInputs()
        {
            return Nodes.Where(x => x.Value.Type == NodeGeneType.Sensor).Select(x => x.Value);
        }

        public void ResetEvaluation()
        {
            foreach (var nodeGene in Nodes.Values)
            {
                nodeGene.Evaluated = false;
                //nodeGene.Value = 0;
            }
        }

        //public Di.Enumerator GetEnumerator()
        //{
        //    return Nodes.Values.GetEnumerator();
        //}
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Zicore.Neat
{
    public class ConnectionGeneCollection
    {
        public void Clear()
        {
            Connections.Clear();
            ConnectionList.Clear();
            outLookup.Clear();
        }

        public Dictionary<int, ConnectionGene> Connections { get; } = new Dictionary<int, ConnectionGene>();

        public List<ConnectionGene> ConnectionList { get; private set; } = new List<ConnectionGene>();
        private readonly Dictionary<(int, int), ConnectionGene> _connectionsInOut = new Dictionary<(int, int), ConnectionGene>();
        private readonly Dictionary<int, ConnectionGene> outLookup = new Dictionary<int, ConnectionGene>();
        
        public bool AddNew(ConnectionGene connection)
        {
            (int, int) key = (connection.Input, connection.Output);
            if (!_connectionsInOut.ContainsKey(key))
            {
                //var connectionCopy = ConnectionGene.Copy(connection, connection.Enabled);
                _connectionsInOut[key] = connection;
                ConnectionList.Add(connection);
                //outLookup.Add(connectionCopy.Output, connectionCopy);
                Connections[connection.InnovationNumber] = connection;
                return true;
            }
            return false;
        }

        public void UpdateInOut((int,int) oldKey, ConnectionGene connection)
        {
            (int, int) newKey = (connection.Input, connection.Output);
            if (_connectionsInOut.ContainsKey(oldKey))
                _connectionsInOut.Remove(oldKey);
            _connectionsInOut.Add(newKey, connection);
        }


        public void UpdateConnection(ConnectionGene connection)
        {
            Connections[connection.InnovationNumber] = connection;
        }

        public int EnabledConnectionCount
        {
            get { return ConnectionList.Count(x => x.Enabled); }
        }

        public ConnectionGene Get((int, int) key)
        {
            return _connectionsInOut[key];
        }

        public ConnectionGene Get(int innovationId)
        {
            return Connections[innovationId];
        }

        public bool Exists((int, int) key)
        {
            return _connectionsInOut.ContainsKey(key);
        }

        public ConnectionGene this[int index]
        {
            get => ConnectionList[index];
            set => ConnectionList[index] = value;
        }

        public ConnectionGene GetConnectionByOutputId(int outputId)
        {
            return outLookup[outputId];
        }

        public int Count => ConnectionList.Count;

        public int MaxInnovationId => Count == 0 ? 0 : Connections.Keys.Max();
    }
}

using Generation.G;
using System.Collections.Generic;
using System.Diagnostics;

namespace Generation.G
{
    internal class GraphRestore : IGraphRestore
    {
        private readonly List<Node> m_nodes_added = new List<Node>();
        private readonly List<Node> m_nodes_removed = new List<Node>();
        private readonly Dictionary<DirectedEdge, RestoreAction> m_connections = new Dictionary<DirectedEdge, RestoreAction>();

        public enum RestoreAction
        {
            Make,
            Break
        }

        public bool CanBeRestored()
        {
            throw new System.NotImplementedException();
        }

        public bool Restore()
        {
            throw new System.NotImplementedException();
        }

        public void AddNode(Node n)
        {
            if (!m_nodes_removed.Remove(n))
            {
                m_nodes_added.Add(n);
            }
        }

        public void Connect(DirectedEdge e)
        {
            RestoreAction ra = m_connections[e];

            if (m_connections.ContainsKey(e))
            {
                // only way we can already know about an edge we are adding is if it was already removed once in the
                // context of this restore point, so the only restore-action it can already have is "break"

                // in which case the net effect of an edge added and removed is nothing
                Debug.Assert(m_connections[e] == RestoreAction.Break);
                m_connections.Remove(e);
            }
            else
            {
                m_connections.Add(e, RestoreAction.Break);
            }
        }

        public void Disconnect(DirectedEdge e)
        {
            if (m_connections.ContainsKey(e))
            {
                // only way we can already know about an edge we are removing is if it was added in the context of this
                // restore point, so the only restore-action it can already have is "break"

                // in which case the net effect of an edge added and removed is nothing
                Debug.Assert(m_connections[e] == RestoreAction.Break);
                m_connections.Remove(e);
            }
            else
            {
                m_connections.Add(e, RestoreAction.Make);
            }
        }

    }
}
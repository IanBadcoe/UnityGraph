using Assets.Generation.G;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;
using UnityEngine;
using System.Linq;

namespace Assets.Generation.G
{
    internal class GraphRestore : IGraphRestore
    {
        private sealed class NodePos
        {
            public readonly Vector2 Pos;
            public readonly INode N;

            public NodePos(INode n, Vector2 pos)
            {
                Pos = pos;
                N = n;
            }
        }

        private readonly Graph m_graph;
        private readonly List<Node> m_nodes_added = new List<Node>();
        private readonly List<Node> m_nodes_removed = new List<Node>();
        private readonly List<NodePos> m_positions = new List<NodePos>();
        private readonly Dictionary<DirectedEdge, RestoreAction> m_connections = new Dictionary<DirectedEdge, RestoreAction>();
        private readonly GraphRestore m_chain_from_restore;
        private GraphRestore m_chain_to_restore;

        public GraphRestore(Graph graph, GraphRestore chain_from_restore)
        {
            m_graph = graph;
            m_chain_from_restore = chain_from_restore;

            Debug.Assert(m_graph == m_chain_from_restore.m_graph);

            // m_chain_from_restore is an older restore than we are, so if it is restored
            // it needs to know that it needs to restore us first...
            if (m_chain_from_restore != null)
            {
                // anything the chain-from used to be chained-to should be already gone,
                // e.g. restored, before we are able to make another new chain
                Debug.Assert(m_chain_from_restore.m_chain_to_restore == null);

                m_chain_from_restore.m_chain_to_restore = this;
            }

            m_positions = m_graph.GetAllNodes().Select(n => new NodePos(n, n.Position)).ToList();
        }

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

        public void RemoveNode(Node node)
        {
            if (!m_nodes_added.Remove(node))
            {
                m_nodes_removed.Add(node);
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
﻿using Assets.Generation.GeomRep;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.G
{
    [System.Diagnostics.DebuggerDisplay("Nodes = {NumNodes}, Edges = {NumEdges}")]
    public class Graph
    {
        private readonly HashSet<Node> m_nodes = new HashSet<Node>();
        private readonly HashSet<DirectedEdge> m_edges = new HashSet<DirectedEdge>();

        public GraphRestore Restore { get; private set; }

        public INode AddNode(string name, string codes, string template, float rad,
                             GeomLayout layout)
        {
            Node n = new Node(name, codes, template, rad, layout);

            if (Restore != null)
            {
                Restore.AddNode(n);
            }

            AddNodeInner(n);

            return n;
        }

        public bool RemoveNode(INode inode)
        {
            if (!Contains(inode))
            {
                return false;
            }

            if (inode.GetConnections().Count > 0)
            {
                return false;
            }

            Node node = (Node)inode;

            if (Restore != null)
            {
                Restore.RemoveNode(node);
            }

            RemoveNodeInner(node);

            return true;
        }

        private void AddNodeInner(Node n)
        {
            m_nodes.Add(n);
        }

        private void RemoveNodeInner(Node node)
        {
            m_nodes.Remove(node);
        }

        public DirectedEdge Connect(INode from, INode to,
                                    float min_length, float max_length, float half_width,
                                    GeomLayout layout)
        {
            if (from == to
                  || !Contains(from)
                  || !Contains(to)
                  || from.Connects(to))
            {
                throw new ArgumentException();
            }

            DirectedEdge temp = new DirectedEdge(from, to, min_length, max_length, half_width, layout);

            if (Restore != null)
            {
                Restore.Connect(temp);
            }

            return ConnectInner(temp);
        }

        public List<INode> GetAllNodes()
        {
            return m_nodes.ToList<INode>();
        }

        public List<DirectedEdge> GetAllEdges()
        {
            return m_edges.ToList();
        }

        public int NumNodes()
        {
            return m_nodes.Count;
        }

        public int NumEdges()
        {
            return m_edges.Count;
        }

        private DirectedEdge ConnectInner(DirectedEdge e)
        {
            Assertion.Assert(!m_edges.Contains(e));

            DirectedEdge real_edge = ((Node)e.Start).Connect((Node)e.End, e.MinLength, e.MaxLength, e.HalfWidth,
                  e.Layout);

            m_edges.Add(real_edge);

            return real_edge;
        }

        public bool Disconnect(INode from, INode to)
        {
            if (!Contains(from) || !Contains(to))
            {
                return false;
            }

            DirectedEdge e = from.GetConnectionTo(to);

            if (e == null)
            {
                return false;
            }

            if (Restore != null)
            {
                Restore.Disconnect(e);
            }

            DisconnectInner(e);

            return true;
        }

        private void DisconnectInner(DirectedEdge e)
        {
            Node n_from = (Node)e.Start;
            Node n_to = (Node)e.End;

            n_from.Disconnect(n_to);

            Assertion.Assert(m_edges.Contains(e));
            m_edges.Remove(e);
        }

        public bool Contains(INode node)
        {
            return m_nodes.Contains((Node)node);
        }

        public bool Contains(DirectedEdge edge)
        {
            return m_edges.Contains(edge);
        }

        public Box2 Bounds()
        {
            Box2 ret = Box2.Empty;

            foreach (INode n in m_nodes)
            {
                Vector2 rad_box = new Vector2(n.Radius, n.Radius);

                ret = ret.Union(new Box2(n.Position - rad_box, n.Position + rad_box));
            }

            return ret;
        }

        public IGraphRestore CreateRestorePoint()
        {
            GraphRestore gr = new GraphRestore(this, Restore);

            Restore = gr;

            return gr;
        }

        public void ClearRestore()
        {
            GraphRestore root = Restore;

            while (root.ChainFrom != null)
            {
                root = root.ChainFrom;
            }

            root.CleanUp();
        }

        public class GraphRestore : IGraphRestore
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
            public GraphRestore ChainFrom { get; }
            public GraphRestore ChainTo { get; private set; }
            private bool m_can_be_restored = true;

            public GraphRestore(Graph graph, GraphRestore chain_from_restore)
            {
                m_graph = graph;
                ChainFrom = chain_from_restore;

                // ChainFrom is an older restore than we are, so if it is restored
                // it needs to know that it needs to restore us first...
                if (ChainFrom != null)
                {
                    // check we're talking about the same graph as the chain we were passed
                    Assertion.Assert(m_graph == ChainFrom.m_graph);

                    // anything the chain-from used to be chained-to should be already gone,
                    // e.g. restored, before we are able to make another new chain
                    Assertion.Assert(ChainFrom.ChainTo == null);

                    ChainFrom.ChainTo = this;
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
                return m_can_be_restored;
            }

            public bool Restore()
            {
                if (!m_can_be_restored)
                {
                    return false;
                }

                if (ChainTo != null)
                {
                    // first undo any newer restore points
                    ChainTo.Restore();
                }

                // disconnect anything we connected
                foreach (var e in m_connections.Keys)
                {
                    RestoreAction ra = m_connections[e];

                    if (ra == RestoreAction.Break)
                    {
                        Assertion.Assert(e.Start.Connects(e.End));

                        m_graph.DisconnectInner(e);
                    }
                }

                // which means we must be able to remove anything we added
                m_nodes_added.ForEach(n => m_graph.RemoveNodeInner(n));

                // put back anything we removed
                m_nodes_removed.ForEach(n => m_graph.AddNodeInner(n));

                // which means we must be able to restore the original connections
                foreach (var e in m_connections.Keys)
                {
                    var ra = m_connections[e];

                    if (ra == RestoreAction.Make)
                    {
                        Assertion.Assert(!e.Start.Connects(e.End));

                        m_graph.ConnectInner(e);
                    }
                }

                int restored_size = m_graph.NumNodes();
                int prev_size = m_positions.Count;

                // putting connections back should leave us the same size as before...
                Assertion.Assert(restored_size == prev_size);

                // and finally put all the positions back
                foreach (NodePos np in m_positions)
                {
                    np.N.Position = np.Pos;
                }

                CleanUp();

                return true;
            }

            public void CleanUp()
            {
                if (ChainTo != null)
                {
                    ChainTo.CleanUp();
                }

                // once we are undone or committed, the user goes back to whatever their previous restore level was
                m_graph.Restore = ChainFrom;

                // we're restored, so whoever might have wanted to chain us mustn't any more
                if (ChainFrom != null)
                {
                    ChainFrom.ChainTo = null;
                }

                m_can_be_restored = false;
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
                if (m_connections.ContainsKey(e))
                {
                    // only way we can already know about an edge we are adding is if it was already removed once in the
                    // context of this restore point, so the only restore-action it can already have is "Make"

                    // in which case the net effect of an edge added and removed is nothing
                    Assertion.Assert(m_connections[e] == RestoreAction.Make);
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
                    // restore point, so the only restore-action it can already have is "Break"

                    // in which case the net effect of an edge removed and added is nothing
                    Assertion.Assert(m_connections[e] == RestoreAction.Break);
                    m_connections.Remove(e);
                }
                else
                {
                    m_connections.Add(e, RestoreAction.Make);
                }
            }

        }
    }
}
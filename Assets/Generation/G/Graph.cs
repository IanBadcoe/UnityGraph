using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Generation.G
{
    public class Graph
    {
        private readonly HashSet<Node> m_nodes = new HashSet<Node>();
        private readonly HashSet<DirectedEdge> m_edges = new HashSet<DirectedEdge>();

        private GraphRestore m_restore;

        public INode AddNode(string name, string codes, string template, double rad /*,
                             GeomLayout.IGeomLayoutCreateFromNode geomCreator */)
        {
            Node n = new Node(name, codes, template/*, geomCreator*/, rad);

            if (m_restore != null)
            {
                m_restore.AddNode(n);
            }

            addNodeInner(n);

            return n;
        }

        private void addNodeInner(Node n)
        {
            m_nodes.Add(n);
        }

        public DirectedEdge Connect(INode from, INode to,
                                    double min_length, double max_length, double half_width/*,
                                    GeomLayout.IGeomLayoutCreateFromDirectedEdge layoutCreator */)
        {
            if (from == to
                  || !Contains(from)
                  || !Contains(to)
                  || from.Connects(to))
                throw new ArgumentException();

            DirectedEdge temp = new DirectedEdge(from, to, min_length, max_length, half_width/*, layoutCreator*/);

            if (m_restore != null)
            {
                m_restore.Connect(temp);
            }

            return ConnectInner(temp);
        }

        internal List<INode> GetAllNodes()
        {
            return m_nodes.ToList<INode>();
        }

        private DirectedEdge ConnectInner(DirectedEdge e)
        {
            Debug.Assert(!m_edges.Contains(e));

            DirectedEdge real_edge = ((Node)e.Start).Connect((Node)e.End, e.MinLength, e.MaxLength, e.HalfWidth/*,
                  e.LayoutCreator*/);

            m_edges.Add(real_edge);

            return real_edge;
        }

        public bool Disconnect(INode from, INode to)
        {
            if (!Contains(from) || !Contains(to))
                return false;

            DirectedEdge e = from.GetConnectionTo(to);

            if (e == null)
                return false;

            if (m_restore != null)
            {
                m_restore.Disconnect(e);
            }

            DisconnectInner(e);

            return true;
        }

        private void DisconnectInner(DirectedEdge e)
        {
            Node n_from = (Node)e.Start;
            Node n_to = (Node)e.End;

            n_from.Disconnect(n_to);

            Debug.Assert(m_edges.Contains(e));
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
    }
}
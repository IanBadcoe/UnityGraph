using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.G
{
    internal class Node : INode
    {
        private readonly string m_codes;
        private readonly string m_template;
        private readonly int m_num;
        private readonly double m_rad;
        private readonly HashSet<DirectedEdge> m_connections = new HashSet<DirectedEdge>();

        private static readonly System.Random s_rand = new System.Random(1);

        public Vector2 Pos { get; set; }

        public string Name { get; private set; }

        public Node(string name, string codes, string template,
             /*GeomLayout.IGeomLayoutCreateFromNode gl_creator, */double rad)
        {
            Name = name;
            m_codes = codes;
            m_template = template;

            m_num = s_rand.Next();

            m_rad = rad;

            // m_gl_creator = gl_creator;
        }

        public UnityEngine.Vector2 GetPos()
        {
            throw new NotImplementedException();
        }

        public bool Connects(INode n)
        {
            throw new NotImplementedException();
        }

        public bool ConnectsForwards(INode to)
        {
            throw new NotImplementedException();
        }

        public bool ConnectsBackwards(INode from)
        {
            throw new NotImplementedException();
        }

        public DirectedEdge Connect(Node n, double min_distance, double max_distance, double width /*,
              GeomLayout.IGeomLayoutCreateFromDirectedEdge layoutCreator*/)
        {
            // cannot multiply connect the same node, forwards or backwards
            if (Connects(n))
                throw new ArgumentException("Cannot multiply connect from '" + Name +
                      "' to '" + n.Name + "'");

            DirectedEdge e = new DirectedEdge(this, n, min_distance, max_distance, width/*, layoutCreator*/);

            Connect(e);
            n.Connect(e);

            return e;
        }

        private void Connect(DirectedEdge e)
        {
            m_connections.Add(e);
        }

        public void Disconnect(Node n)
        {
            if (!Connects(n))
                return;

            // simplest just to try removing the forward and reverse edges
            // only the nodes are part of the edge identity
            m_connections.Remove(new DirectedEdge(this, n, 0, 0, 0/*, null*/));
            m_connections.Remove(new DirectedEdge(n, this, 0, 0, 0/*, null*/));

            n.Disconnect(this);
        }

        public DirectedEdge GetConnectionTo(INode node)
        {
            throw new NotImplementedException();
        }

        public DirectedEdge GetConnectionFrom(INode from)
        {
            throw new NotImplementedException();
        }
    }
}
using Assets.Generation.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.G
{
    public class Node : INode
    {
        private readonly int m_num;
        private readonly HashSet<DirectedEdge> m_connections = new HashSet<DirectedEdge>();

        private static readonly ClRand s_rand = new ClRand(1);

        public string Name { get; }
        public string Codes { get; }
        public string Template { get; }
        public float Radius { get; }

        public Vector2 Pos { get; set; }
        public uint Colour { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Force { get; set; }

        public Node(string name, string codes, string template,
             /*GeomLayout.IGeomLayoutCreateFromNode gl_creator, */float rad)
        {
            Name = name;
            Codes = codes;
            Template = template;

            m_num = s_rand.Next();

            Radius = rad;

            // m_gl_creator = gl_creator;
        }

        public bool Connects(INode n)
        {
            return ConnectsForwards(n) || ConnectsBackwards(n);
        }

        public bool ConnectsForwards(INode to)
        {
            return m_connections.Contains(new DirectedEdge(this, to, 0, 0, 0/*, null*/));
        }

        public bool ConnectsBackwards(INode from)
        {
            return m_connections.Contains(new DirectedEdge(from, this, 0, 0, 0/*, null*/));
        }

        public DirectedEdge Connect(Node n, float min_distance, float max_distance, float width /*,
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

        public DirectedEdge GetConnectionTo(INode to)
        {
            foreach (DirectedEdge e in m_connections)
            {
                if (e.End == to)
                    return e;
            }

            return null;
        }

        public DirectedEdge GetConnectionFrom(INode from)
        {
            foreach (DirectedEdge e in m_connections)
            {
                if (e.Start == from)
                    return e;
            }

            return null;
        }

        public IReadOnlyList<DirectedEdge> GetConnections()
        {
            return m_connections.ToList();
        }

        public IReadOnlyList<DirectedEdge> GetInConnections()
        {
            return m_connections.Where(c => c.End == this).ToList();
        }

        public IReadOnlyList<DirectedEdge> GetOutConnections()
        {
            return m_connections.Where(c => c.Start == this).ToList();
        }

        public float Step(float t)
        {
            Vector2 d = Force * t;
            Position += d;

            return d.magnitude;
        }
    }
}
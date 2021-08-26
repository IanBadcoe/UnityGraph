using Assets.Generation.Gen;
using Assets.Generation.GeomRep;
using Assets.Generation.Templates;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.G
{
    [System.Diagnostics.DebuggerDisplay("Name = {Name}")]
    public class Node : IRelaxationParamSource, IHMChild
    {
        private readonly HashSet<DirectedEdge> m_connections = new HashSet<DirectedEdge>();

        public GeomLayout Layout { get; }

        public string Name { get; }
        public string Codes { get; }
        public float Radius { get; }
        public float WallThickness { get; }

        public Vector2 Pos { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Force { get; set; }

        HierarchyMetadata m_parent;
        public HierarchyMetadata Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                if (m_parent != null)
                {
                    m_parent.Children.Remove(this);
                }

                m_parent = value;

                if (m_parent != null)
                {
                    m_parent.Children.Add(this);
                }
            }
        }

        public IList<IHMChild> Children { get; }

        public Node(string name, string codes, float rad, float wall_thickness = 0, HierarchyMetadata parent = null)
            : this(name, codes, rad, wall_thickness, null, parent)
        {
        }

        public Node(string name, string codes,
            float rad, float wall_thickness,
            GeomLayout layout, HierarchyMetadata parent = null)
        {
            Name = name;
            Codes = codes;

            Radius = rad;
            WallThickness = wall_thickness;     // zero means no wall

            Layout = layout;

            Parent = parent;

            Children = new List<IHMChild>();

            // mere edge joins do not have sizes
            Assertion.Assert(!Codes.Contains("j") || Radius == 0);
            Assertion.Assert(!Codes.Contains("j") || WallThickness == 0);
        }

        public bool Connects(Node n)
        {
            return ConnectsForwards(n) || ConnectsBackwards(n);
        }

        public bool ConnectsForwards(Node to)
        {
            return m_connections.Contains(new DirectedEdge(this, to));
        }

        public bool ConnectsBackwards(Node from)
        {
            return m_connections.Contains(new DirectedEdge(from, this));
        }

        public DirectedEdge Connect(Node n, float min_distance, float max_distance, float width,
              GeomLayout layout = null, float wall_thickness = 0)
        {
            // cannot multiply connect the same node, forwards or backwards
            if (Connects(n))
            {
                throw new ArgumentException("Cannot multiply connect from '" + Name +
                      "' to '" + n.Name + "'");
            }

            DirectedEdge e = new DirectedEdge(this, n, min_distance, max_distance, width, wall_thickness, layout);

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
            {
                return;
            }

            // simplest just to try removing the forward and reverse edges
            // only the nodes are part of the edge identity
            m_connections.Remove(new DirectedEdge(this, n));
            m_connections.Remove(new DirectedEdge(n, this));

            n.Disconnect(this);
        }

        public DirectedEdge GetConnectionTo(Node to)
        {
            foreach (DirectedEdge e in m_connections)
            {
                if (e.End == to)
                {
                    return e;
                }
            }

            return null;
        }

        public DirectedEdge GetConnectionFrom(Node from)
        {
            foreach (DirectedEdge e in m_connections)
            {
                if (e.Start == from)
                {
                    return e;
                }
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

        public int NumConnections()
        {
            return m_connections.Count;
        }

        public int GetParams(List<double> list, int offset)
        {
            list.Add(Position.x);
            list.Add(Position.y);

            return 2;
        }

        public int SetParams(double[] array, int offset)
        {
            Position = new Vector2((float)array[offset + 0], (float)array[offset + 1]);

            return 2;
        }

        public bool IsChildNode(Node n)
        {
            return ReferenceEquals(n, this);
        }
    }
}
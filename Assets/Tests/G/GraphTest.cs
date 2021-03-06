using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.Templates;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphTest
{
    [Test]
    public void TestAddNode()
    {
        Graph g = new Graph();

        Node n = g.AddNode("n", "x", 10, CircularGeomLayout.Instance);

        Assert.AreEqual(1, g.NumNodes());
        Assert.AreEqual(0, g.NumEdges());
        Assert.IsTrue(g.Contains(n));

        Assert.AreEqual("n", n.Name);
        Assert.AreEqual("x", n.Codes);
        Assert.AreEqual(10.0, n.Radius, 0);
        Assert.AreEqual(CircularGeomLayout.Instance, n.Layout);

        Node n2 = g.AddNode("n2", "x2", 20, null);

        Assert.AreEqual(2, g.NumNodes());
        Assert.AreEqual(0, g.NumEdges());
        Assert.IsTrue(g.Contains(n2));
        Assert.IsTrue(g.Contains(n));

        Assert.AreEqual("n2", n2.Name);
        Assert.AreEqual("x2", n2.Codes);
        Assert.AreEqual(20.0, n2.Radius, 0);
        Assert.AreEqual(null, n2.Layout);
    }

    [Test]
    public void TestRemoveNode()
    {
        Graph g = new Graph();

        Node n = g.AddNode("n", "x", 10, null);
        Node n2 = g.AddNode("n2", "x2", 20, null);
        g.Connect(n, n2, 0, 0, null);

        Assert.AreEqual(2, g.NumNodes());
        Assert.AreEqual(1, g.NumEdges());
        Assert.IsTrue(g.Contains(n2));
        Assert.IsTrue(g.Contains(n));

        // cannot remove a connected node
        Assert.IsFalse(g.RemoveNode(n));
        Assert.IsFalse(g.RemoveNode(n2));

        Assert.AreEqual(2, g.NumNodes());
        Assert.IsTrue(g.Contains(n2));
        Assert.IsTrue(g.Contains(n));

        g.Disconnect(n, n2);
        Assert.AreEqual(0, g.NumEdges());

        // cannot remove a node we never heard of
        Assert.IsFalse(g.RemoveNode(new Node("", "", 0)));

        Assert.IsTrue(g.RemoveNode(n));
        Assert.AreEqual(1, g.NumNodes());
        Assert.IsTrue(g.Contains(n2));
        Assert.IsFalse(g.Contains(n));

        Assert.IsTrue(g.RemoveNode(n2));
        Assert.AreEqual(0, g.NumNodes());
        Assert.IsFalse(g.Contains(n2));
    }

    [Test]
    public void TestConnect()
    {
        Graph g = new Graph();

        Assert.AreEqual(0, g.NumEdges());

        Node n = g.AddNode("n", "x", 10, null);
        Node n2 = g.AddNode("n2", "x2", 20, null);

        Assert.IsFalse(n.Connects(n2));
        Assert.IsFalse(n2.Connects(n));
        Assert.AreEqual(0, g.NumEdges());

        Assert.NotNull(g.Connect(n, n2, 2, 3, null));
        Assert.AreEqual(1, g.NumEdges());
        Assert.IsTrue(n.Connects(n2));
        Assert.IsTrue(n2.Connects(n));

        DirectedEdge e = n.GetConnectionTo(n2);
        Assert.IsNotNull(e);
        Assert.AreEqual(n, e.Start);
        Assert.AreEqual(n2, e.End);
        Assert.AreEqual(1, e.MinLength, 0);
        Assert.AreEqual(2, e.MaxLength, 0);
        Assert.AreEqual(3, e.HalfWidth, 0);
    }

    [Test]
    public void TestDisconnect()
    {
        Graph g = new Graph();

        // cannot disconnect two unknown nodes
        Assert.IsFalse(g.Disconnect(new Node("", "", 0), new Node("", "", 0)));

        Node n = g.AddNode("n", "x", 10, null);

        {
            // cannot disconnect a node we know and one we don't
            Node dummy = new Node("", "", 0);
            Assert.IsFalse(g.Disconnect(n, dummy));
        }

        Node n2 = g.AddNode("n2", "x2", 20, null);

        {
            // cannot disconnect two unconnected nodes
            Assert.IsFalse(g.Disconnect(n, n2));
        }

        g.Connect(n, n2, 0, 0, null);
        Assert.AreEqual(1, g.NumEdges());
        Assert.IsTrue(n.Connects(n2));

        Assert.IsTrue(g.Disconnect(n, n2));
        Assert.AreEqual(0, g.NumEdges());
        Assert.IsFalse(n.Connects(n2));

        // it won't disconnect again...
        Assert.IsFalse(g.Disconnect(n, n2));
    }

    [Test]
    public void testAllGraphEdges()
    {
        Graph g = new Graph();
        Node n = g.AddNode("n", "", 0, null);
        Node m = g.AddNode("m", "", 0, null);
        Node o = g.AddNode("o", "", 0, null);

        Assert.AreEqual(0, g.NumEdges());

        g.Connect(n, m, 0, 0, null);

        Assert.AreEqual(1, g.NumEdges());
        Assert.IsTrue(g.GetAllEdges().Contains(new DirectedEdge(n, m)));

        g.Connect(m, o, 0, 0, null);

        Assert.AreEqual(2, g.NumEdges());
        Assert.IsTrue(g.GetAllEdges().Contains(new DirectedEdge(m, o)));

        g.Connect(o, n, 0, 0, null);

        Assert.AreEqual(3, g.NumEdges());
        Assert.IsTrue(g.GetAllEdges().Contains(new DirectedEdge(o, n)));

        g.Disconnect(n, m);

        Assert.AreEqual(2, g.NumEdges());
        Assert.IsFalse(g.GetAllEdges().Contains(new DirectedEdge(n, m)));
    }

    [Test]
    public void TestAllGraphNodes()
    {
        Graph g = new Graph();

        Assert.AreEqual(0, g.NumNodes());

        Node n = g.AddNode("n", "", 0, null);

        Assert.AreEqual(1, g.NumNodes());
        Assert.IsTrue(g.GetAllNodes().Contains(n));

        Node m = g.AddNode("m", "", 0, null);

        Assert.AreEqual(2, g.NumNodes());
        Assert.IsTrue(g.GetAllNodes().Contains(m));

        Node o = g.AddNode("o", "", 0, null);

        Assert.AreEqual(3, g.NumNodes());
        Assert.IsTrue(g.GetAllNodes().Contains(o));

        g.RemoveNode(n);

        Assert.AreEqual(2, g.NumNodes());
        Assert.IsFalse(g.GetAllNodes().Contains(n));
    }

    class GraphRecord
    {
        readonly HashSet<Node> m_nodes;
        readonly HashSet<DirectedEdge> m_edges = new HashSet<DirectedEdge>();
        readonly Dictionary<Node, Vector2> m_positions = new Dictionary<Node, Vector2>();
        readonly Dictionary<Node, HierarchyMetadata> m_metas = new Dictionary<Node, HierarchyMetadata>();

        public GraphRecord(Graph g)
        {
            m_nodes = new HashSet<Node>(g.GetAllNodes());
            m_edges = new HashSet<DirectedEdge>(g.GetAllEdges());

            foreach (Node n in m_nodes)
            {
                m_positions.Add(n, n.Position);
                m_metas.Add(n, n.Parent);
            }
        }

        public bool Compare(Graph g)
        {
            if (m_nodes.Count != g.NumNodes())
            {
                return false;
            }

            if (!m_nodes.SetEquals(new HashSet<Node>(g.GetAllNodes())))
            {
                return false;
            }

            if (m_edges.Count != g.NumEdges())
            {
                return false;
            }

            if (!m_edges.SetEquals(new HashSet<DirectedEdge>(g.GetAllEdges())))
            {
                return false;
            }

            foreach (Node n in g.GetAllNodes())
            {
                if (n.Position != m_positions[n])
                {
                    return false;
                }

                if (n.Parent != m_metas[n])
                {
                    return false;
                }

                foreach (DirectedEdge e in n.GetConnections())
                {
                    if (!m_edges.Contains(e))
                    {
                        return false;
                    }
                }
            }

            // check that the nodes know about the connections
            foreach (DirectedEdge e in m_edges)
            {
                Node start = e.Start;
                Node end = e.End;

                if (!start.ConnectsForwards(end))
                {
                    return false;
                }

                if (!end.ConnectsBackwards(start))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Test]
    public void TestGraphRecord()
    {
        // same if empty
        {
            Graph g = new Graph();

            GraphRecord gr = new GraphRecord(g);

            Assert.IsTrue(gr.Compare(g));
        }

        // same if one node
        {
            Graph g = new Graph();

            g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            Assert.IsTrue(gr.Compare(g));
        }

        // different if one node but different graphs
        // (because node-identity is based on object-identity
        //  we could move to node property comparison of some sort if we ever need cross-graph comparisons...)
        {
            Graph g = new Graph();
            g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            Graph g1 = new Graph();
            g1.AddNode("", "", 0, null);

            Assert.IsFalse(gr.Compare(g1));
        }

        // same if two nodes and an edge
        {
            Graph g = new Graph();

            Node n1 = g.AddNode("", "", 0, null);

            Node n2 = g.AddNode("", "", 0, null);

            g.Connect(n1, n2, 0, 0, null);

            GraphRecord gr = new GraphRecord(g);

            Assert.IsTrue(gr.Compare(g));
        }

        // same if node added and removed
        {
            Graph g = new Graph();

            GraphRecord gr = new GraphRecord(g);

            Node n1 = g.AddNode("", "", 0, null);
            g.RemoveNode(n1);

            Assert.IsTrue(gr.Compare(g));
        }

        // same if edge added and removed
        {
            Graph g = new Graph();
            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            g.Connect(n1, n2, 0, 0, null);
            g.Disconnect(n1, n2);

            Assert.IsTrue(gr.Compare(g));
        }

        // different if node added
        {
            Graph g = new Graph();

            GraphRecord gr = new GraphRecord(g);

            g.AddNode("", "", 0, null);

            Assert.IsFalse(gr.Compare(g));
        }

        // different if edge added
        {
            Graph g = new Graph();
            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            g.Connect(n1, n2, 0, 0, null);

            Assert.IsFalse(gr.Compare(g));
        }

        // different if node moved
        {
            Graph g = new Graph();
            Node n1 = g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            n1.Position = new Vector2(1, 0);

            Assert.IsFalse(gr.Compare(g));
        }

        // different if metadata set
        {
            Graph g = new Graph();
            Node n1 = g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            n1.Parent = new HierarchyMetadata(null, null);

            Assert.IsFalse(gr.Compare(g));
        }
    }

    [Test]
    public void TestRestore()
    {
        // nop
        {
            Graph g = new Graph();

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            Assert.IsTrue(igr.Restore());
            // restores only work once...
            Assert.IsFalse(igr.Restore());

            Assert.IsTrue(gr.Compare(g));
        }

        // add node to empty
        {
            Graph g = new Graph();

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.AddNode("", "", 0, null);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // add nodes and edge to empty
        {
            Graph g = new Graph();

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            g.Connect(n1, n2, 0, 0, null);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // get back removed nodes and edges
        {
            Graph g = new Graph();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            g.Connect(n1, n2, 0, 0, null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Disconnect(n1, n2);

            g.RemoveNode(n1);
            g.RemoveNode(n2);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // add and remove a node shouldn't break anything
        {
            Graph g = new Graph();

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            Node n1 = g.AddNode("", "", 0, null, new HierarchyMetadata(null, null));

            g.RemoveNode(n1);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // add and remove an edge shouldn't break anything
        {
            Graph g = new Graph();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Connect(n1, n2, 0, 0, null);
            g.Disconnect(n1, n2);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // add and remove and re-add an edge shouldn't break anything
        {
            Graph g = new Graph();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Connect(n1, n2, 0, 0, null);
            g.Disconnect(n1, n2);
            g.Connect(n1, n2, 0, 0, null);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // remove and add an edge shouldn't break anything
        {
            Graph g = new Graph();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);
            g.Connect(n1, n2, 0, 0, null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Disconnect(n1, n2);
            g.Connect(n1, n2, 0, 0, null);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // remove and add and re-remove an edge shouldn't break anything
        {
            Graph g = new Graph();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);
            g.Connect(n1, n2, 0, 0, null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Disconnect(n1, n2);
            g.Connect(n1, n2, 0, 0, null);
            g.Disconnect(n1, n2);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // multiple restore, unchained
        {
            Graph g = new Graph();

            GraphRecord gr1 = new GraphRecord(g);

            IGraphRestore igr1 = g.CreateRestorePoint();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            g.Connect(n1, n2, 0, 0, null);

            GraphRecord gr2 = new GraphRecord(g);

            IGraphRestore igr2 = g.CreateRestorePoint();

            Node n3 = g.AddNode("", "", 0, null);
            Node n4 = g.AddNode("", "", 0, null);

            g.Connect(n3, n4, 0, 0, null);

            Assert.AreEqual(igr2, g.Restore);

            igr2.Restore();

            Assert.IsFalse(igr2.CanBeRestored());
            Assert.IsTrue(igr1.CanBeRestored());
            Assert.AreEqual(igr1, g.Restore);

            Assert.IsTrue(gr2.Compare(g));

            igr1.Restore();
            Assert.IsFalse(igr1.CanBeRestored());

            Assert.IsTrue(gr1.Compare(g));
        }

        // chained restore
        {
            Graph g = new Graph();

            GraphRecord gr1 = new GraphRecord(g);

            IGraphRestore igr1 = g.CreateRestorePoint();

            Node n1 = g.AddNode("", "", 0, null);
            Node n2 = g.AddNode("", "", 0, null);

            g.Connect(n1, n2, 0, 0, null);

            IGraphRestore igr2 = g.CreateRestorePoint();

            Node n3 = g.AddNode("", "", 0, null);
            Node n4 = g.AddNode("", "", 0, null);

            g.Connect(n3, n4, 0, 0, null);

            igr1.Restore();
            Assert.IsFalse(igr2.CanBeRestored());
            Assert.AreEqual(null, g.Restore);

            Assert.IsTrue(gr1.Compare(g));
        }

        // restore to intermediate point then start a new restore
        {
            Graph g = new Graph();

            GraphRecord gr1 = new GraphRecord(g);

            IGraphRestore igr1 = g.CreateRestorePoint();

            g.AddNode("", "", 0, null);

            GraphRecord gr2 = new GraphRecord(g);

            IGraphRestore igr2 = g.CreateRestorePoint();

            g.AddNode("", "", 0, null);

            igr2.Restore();

            Assert.AreEqual(igr1, g.Restore);
            Assert.IsFalse(igr2.CanBeRestored());
            Assert.IsTrue(igr1.CanBeRestored());

            Assert.IsTrue(gr2.Compare(g));

            IGraphRestore igr3 = g.CreateRestorePoint();

            g.AddNode("", "", 0, null);

            igr1.Restore();

            Assert.IsTrue(gr1.Compare(g));

            Assert.IsFalse(igr1.CanBeRestored());
            Assert.IsFalse(igr3.CanBeRestored());
            Assert.AreEqual(null, g.Restore);
        }

        // keep a restore but then abandon it, committing to all our changes

        // restore to intermediate point then start a new restore
        {
            Graph g = new Graph();

            IGraphRestore igr1 = g.CreateRestorePoint();

            g.AddNode("", "", 0, null);

            IGraphRestore igr2 = g.CreateRestorePoint();

            g.AddNode("", "", 0, null);

            IGraphRestore igr3 = g.CreateRestorePoint();

            g.AddNode("", "", 0, null);

            GraphRecord gr1 = new GraphRecord(g);

            g.ClearRestore();

            // should still have all our changes
            Assert.IsTrue(gr1.Compare(g));
            // and all the restores shoudl know they are now invalid
            Assert.IsFalse(igr1.CanBeRestored());
            Assert.IsFalse(igr2.CanBeRestored());
            Assert.IsFalse(igr3.CanBeRestored());
        }
    }

    [Test]
    public void TestBounds()
    {
        Graph g = new Graph();

        Assert.IsTrue(g.Bounds().Equals(new Box2()));

        Node n1 = g.AddNode("", "", 1.0f, null);

        Assert.IsTrue(g.Bounds().Equals(new Box2(new Vector2(-1, -1), new Vector2(1, 1))));

        Node n2 = g.AddNode("", "", 2.0f, null);

        Assert.IsTrue(g.Bounds().Equals(new Box2(new Vector2(-2, -2), new Vector2(2, 2))));

        n1.Position = new Vector2(-2, 0);

        Assert.IsTrue(g.Bounds().Equals(new Box2(new Vector2(-3, -2), new Vector2(2, 2))));

        n2.Position = new Vector2(10, 10);

        Assert.IsTrue(g.Bounds().Equals(new Box2(new Vector2(-3, -1), new Vector2(12, 12))));
    }

    //   [Test]
    //   public void testPrint()
    //{
    //    {
    //        Graph g = new Graph();

    //        g.AddNode("xx", "yy", "zz", 1.0);
    //        g.AddNode("aa", "bb", "cc", 1.0);

    //        String s = g.print();

    //        Assert.IsTrue(s.contains("xx"));
    //        Assert.IsTrue(s.contains("yy"));
    //        Assert.IsTrue(s.contains("zz"));
    //        Assert.IsTrue(s.contains("aa"));
    //        Assert.IsTrue(s.contains("bb"));
    //        Assert.IsTrue(s.contains("cc"));
    //        Assert.IsTrue(s.contains("{"));
    //        Assert.IsTrue(s.contains("}"));
    //    }

    //    {
    //        Graph g = new Graph();

    //        Node n1 = g.AddNode("xx", "yy", "zz", 1.0);
    //        Node n2 = g.AddNode("aa", "bb", "cc", 1.0);
    //        g.Connect(n1, n2, 0, 0, 0);

    //        String s = g.print();

    //        String splits[] = s.split("[\\{\\}]");

    //        Debug.Assert(Equals(5, splits.length);

    //        // after the last closing "}" wer have only a linefeed
    //        Debug.Assert(Equals("", splits[4].trim());

    //        // the two nodes can come out in any order
    //        int first = splits[0].contains("aa") ? 0 : 2;
    //        int second = 2 - first;

    //        Assert.IsTrue(splits[first].contains("aa"));
    //        Assert.IsTrue(splits[second].contains("xx"));

    //        // each node should be followed by a connects block that mentions the other
    //        Assert.IsTrue(splits[first + 1].contains("xx"));
    //        Assert.IsTrue(splits[second + 1].contains("aa"));
    //    }
    //}

    [Test]
    public void TestConnect_Exceptions()
    {
        Graph g = new Graph();

        // cannot Connect two nodes we never neard of
        Assert.Throws<ArgumentException>(() => g.Connect(new Node("", "", 0),
              new Node("", "", 0), 0, 0, null));

        Node n = g.AddNode("n", "x", 10, null);

        // cannot Connect a node we know and one we don't
        Assert.Throws<ArgumentException>(() => g.Connect(n, new Node("", "", 0), 0, 0, null));
    }
}
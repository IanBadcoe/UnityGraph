using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Generation.G;
using System;
using Assets.Generation.GeomRep;

public class GraphTest
{
    [Test]
    public void TestAddNode()
    {
        Graph g = new Graph(null);

        INode n = g.AddNode("n", "x", "y", 10);

        Assert.AreEqual(1, g.NumNodes());
        Assert.AreEqual(0, g.NumEdges());
        Assert.IsTrue(g.Contains(n));

        Assert.AreEqual("n", n.Name);
        Assert.AreEqual("x", n.Codes);
        Assert.AreEqual("y", n.Template);
        Assert.AreEqual(10.0, n.Radius, 0);

        INode n2 = g.AddNode("n2", "x2", "y2", 20);

        Assert.AreEqual(2, g.NumNodes());
        Assert.AreEqual(0, g.NumEdges());
        Assert.IsTrue(g.Contains(n2));
        Assert.IsTrue(g.Contains(n));

        Assert.AreEqual("n2", n2.Name);
        Assert.AreEqual("x2", n2.Codes);
        Assert.AreEqual("y2", n2.Template);
        Assert.AreEqual(20.0, n2.Radius, 0);
    }

    [Test]
    public void TestRemoveNode()
    {
        Graph g = new Graph(null);

        INode n = g.AddNode("n", "x", "y", 10);
        INode n2 = g.AddNode("n2", "x2", "y2", 20);
        g.Connect(n, n2, 0, 0, 0);

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
        Assert.IsFalse(g.RemoveNode(new Node("", "", "", 0)));

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
        Graph g = new Graph(null);

        Assert.AreEqual(0, g.NumEdges());

        INode n = g.AddNode("n", "x", "y", 10);
        INode n2 = g.AddNode("n2", "x2", "y2", 20);

        Assert.IsFalse(n.Connects(n2));
        Assert.IsFalse(n2.Connects(n));
        Assert.AreEqual(0, g.NumEdges());

        Assert.NotNull(g.Connect(n, n2, 1, 2, 3));
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
        Graph g = new Graph(null);

        // cannot disconnect two unknown nodes
        Assert.IsFalse(g.Disconnect(new Node("", "", "", 0), new Node("", "", "", 0)));

        INode n = g.AddNode("n", "x", "y", 10);

        {
            // cannot disconnect a node we know and one we don't
            INode dummy = new Node("", "", "", 0);
            Assert.IsFalse(g.Disconnect(n, dummy));
        }

        INode n2 = g.AddNode("n2", "x2", "y2", 20);

        {
            // cannot disconnect two unconnected nodes
            Assert.IsFalse(g.Disconnect(n, n2));
        }

        g.Connect(n, n2, 0, 0, 0);
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
        Graph g = new Graph(null);
        INode n = g.AddNode("n", "", "", 0);
        INode m = g.AddNode("m", "", "", 0);
        INode o = g.AddNode("o", "", "", 0);

        Assert.AreEqual(0, g.NumEdges());

        g.Connect(n, m, 0, 0, 0);

        Assert.AreEqual(1, g.NumEdges());
        Assert.IsTrue(g.GetAllEdges().Contains(new DirectedEdge(n, m, 0, 0, 0)));

        g.Connect(m, o, 0, 0, 0);

        Assert.AreEqual(2, g.NumEdges());
        Assert.IsTrue(g.GetAllEdges().Contains(new DirectedEdge(m, o, 0, 0, 0)));

        g.Connect(o, n, 0, 0, 0);

        Assert.AreEqual(3, g.NumEdges());
        Assert.IsTrue(g.GetAllEdges().Contains(new DirectedEdge(o, n, 0, 0, 0)));

        g.Disconnect(n, m);

        Assert.AreEqual(2, g.NumEdges());
        Assert.IsFalse(g.GetAllEdges().Contains(new DirectedEdge(n, m, 0, 0, 0)));
    }

    [Test]
    public void TestAllGraphNodes()
    {
        Graph g = new Graph(null);

        Assert.AreEqual(0, g.NumNodes());

        INode n = g.AddNode("n", "", "", 0);

        Assert.AreEqual(1, g.NumNodes());
        Assert.IsTrue(g.GetAllNodes().Contains(n));

        INode m = g.AddNode("m", "", "", 0);

        Assert.AreEqual(2, g.NumNodes());
        Assert.IsTrue(g.GetAllNodes().Contains(m));

        INode o = g.AddNode("o", "", "", 0);

        Assert.AreEqual(3, g.NumNodes());
        Assert.IsTrue(g.GetAllNodes().Contains(o));

        g.RemoveNode(n);

        Assert.AreEqual(2, g.NumNodes());
        Assert.IsFalse(g.GetAllNodes().Contains(n));
    }

    class GraphRecord
    {
        readonly HashSet<INode> m_nodes;
        readonly HashSet<DirectedEdge> m_edges = new HashSet<DirectedEdge>();
        readonly Dictionary<INode, Vector2> m_positions = new Dictionary<INode, Vector2>();

        public GraphRecord(Graph g)
        {
            m_nodes = new HashSet<INode>(g.GetAllNodes());
            m_edges = new HashSet<DirectedEdge>(g.GetAllEdges());

            foreach (INode n in m_nodes)
            {
                m_positions.Add(n, n.Position);
            }
        }

        public bool Compare(Graph g)
        {
            if (m_nodes.Count != g.NumNodes())
                return false;

            if (!m_nodes.SetEquals(new HashSet<INode>(g.GetAllNodes())))
                return false;

            if (m_edges.Count != g.NumEdges())
                return false;

            if (!m_edges.SetEquals(new HashSet<DirectedEdge>(g.GetAllEdges())))
                return false;

            foreach (INode n in g.GetAllNodes())
            {
                if (n.Position != m_positions[n])
                    return false;

                foreach (DirectedEdge e in n.GetConnections())
                {
                    if (!m_edges.Contains(e))
                        return false;
                }
            }

            // check that the nodes know about the connections
            foreach (DirectedEdge e in m_edges)
            {
                Node start = (Node)e.Start;
                Node end = (Node)e.End;

                if (!start.ConnectsForwards(end))
                    return false;

                if (!end.ConnectsBackwards(start))
                    return false;
            }

            return true;
        }
    }

    [Test]
    public void TestGraphRecord()
    {
        // same if empty
        {
            Graph g = new Graph(null);

            GraphRecord gr = new GraphRecord(g);

            Assert.IsTrue(gr.Compare(g));
        }

        // same if one node
        {
            Graph g = new Graph(null);

            g.AddNode("", "", "", 0);

            GraphRecord gr = new GraphRecord(g);

            Assert.IsTrue(gr.Compare(g));
        }

        // different if one node but different graphs
        // (because node-identity is based on object-identity
        //  we could move to node property comparison of some sort if we ever need cross-graph comparisons...)
        {
            Graph g = new Graph(null);
            g.AddNode("", "", "", 0);

            GraphRecord gr = new GraphRecord(g);

            Graph g1 = new Graph(null);
            g1.AddNode("", "", "", 0);

            Assert.IsFalse(gr.Compare(g1));
        }

        // same if two nodes and an edge
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("", "", "", 0);

            INode n2 = g.AddNode("", "", "", 0);

            g.Connect(n1, n2, 0, 0, 0);

            GraphRecord gr = new GraphRecord(g);

            Assert.IsTrue(gr.Compare(g));
        }

        // same if node added and removed
        {
            Graph g = new Graph(null);

            GraphRecord gr = new GraphRecord(g);

            INode n1 = g.AddNode("", "", "", 0);
            g.RemoveNode(n1);

            Assert.IsTrue(gr.Compare(g));
        }

        // same if edge added and removed
        {
            Graph g = new Graph(null);
            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            GraphRecord gr = new GraphRecord(g);

            g.Connect(n1, n2, 0, 0, 0);
            g.Disconnect(n1, n2);

            Assert.IsTrue(gr.Compare(g));
        }

        // different if node added
        {
            Graph g = new Graph(null);

            GraphRecord gr = new GraphRecord(g);

            g.AddNode("", "", "", 0);

            Assert.IsFalse(gr.Compare(g));
        }

        // different if edge added
        {
            Graph g = new Graph(null);
            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            GraphRecord gr = new GraphRecord(g);

            g.Connect(n1, n2, 0, 0, 0);

            Assert.IsFalse(gr.Compare(g));
        }

        // different if node moved
        {
            Graph g = new Graph(null);
            INode n1 = g.AddNode("", "", "", 0);

            GraphRecord gr = new GraphRecord(g);

            n1.Position = new Vector2(1, 0);

            Assert.IsFalse(gr.Compare(g));
        }
    }

    [Test]
    public void TestRestore()
    {
        // nop
        {
            Graph g = new Graph(null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            Assert.IsTrue(igr.Restore());
            // restores only work once...
            Assert.IsFalse(igr.Restore());

            Assert.IsTrue(gr.Compare(g));
        }

        // add node to empty
        {
            Graph g = new Graph(null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.AddNode("", "", "", 0);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // add nodes and edge to empty
        {
            Graph g = new Graph(null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            g.Connect(n1, n2, 0, 0, 0);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // get back removed nodes and edges
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            g.Connect(n1, n2, 0, 0, 0);

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
            Graph g = new Graph(null);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            INode n1 = g.AddNode("", "", "", 0);

            g.RemoveNode(n1);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // add and remove an edge shouldn't break anything
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Connect(n1, n2, 0, 0, 0);
            g.Disconnect(n1, n2);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // add and remove and re-add an edge shouldn't break anything
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Connect(n1, n2, 0, 0, 0);
            g.Disconnect(n1, n2);
            g.Connect(n1, n2, 0, 0, 0);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // remove and add an edge shouldn't break anything
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);
            g.Connect(n1, n2, 0, 0, 0);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Disconnect(n1, n2);
            g.Connect(n1, n2, 0, 0, 0);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // remove and add and re-remove an edge shouldn't break anything
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);
            g.Connect(n1, n2, 0, 0, 0);

            GraphRecord gr = new GraphRecord(g);

            IGraphRestore igr = g.CreateRestorePoint();

            g.Disconnect(n1, n2);
            g.Connect(n1, n2, 0, 0, 0);
            g.Disconnect(n1, n2);

            igr.Restore();

            Assert.IsTrue(gr.Compare(g));
        }

        // multiple restore, unchained
        {
            Graph g = new Graph(null);

            GraphRecord gr1 = new GraphRecord(g);

            IGraphRestore igr1 = g.CreateRestorePoint();

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            g.Connect(n1, n2, 0, 0, 0);

            GraphRecord gr2 = new GraphRecord(g);

            IGraphRestore igr2 = g.CreateRestorePoint();

            INode n3 = g.AddNode("", "", "", 0);
            INode n4 = g.AddNode("", "", "", 0);

            g.Connect(n3, n4, 0, 0, 0);

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
            Graph g = new Graph(null);

            GraphRecord gr1 = new GraphRecord(g);

            IGraphRestore igr1 = g.CreateRestorePoint();

            INode n1 = g.AddNode("", "", "", 0);
            INode n2 = g.AddNode("", "", "", 0);

            g.Connect(n1, n2, 0, 0, 0);

            IGraphRestore igr2 = g.CreateRestorePoint();

            INode n3 = g.AddNode("", "", "", 0);
            INode n4 = g.AddNode("", "", "", 0);

            g.Connect(n3, n4, 0, 0, 0);

            igr1.Restore();
            Assert.IsFalse(igr2.CanBeRestored());
            Assert.AreEqual(null, g.Restore);

            Assert.IsTrue(gr1.Compare(g));
        }

        // restore to intermediate point then start a new restore
        {
            Graph g = new Graph(null);

            GraphRecord gr1 = new GraphRecord(g);

            IGraphRestore igr1 = g.CreateRestorePoint();

            g.AddNode("", "", "", 0);

            GraphRecord gr2 = new GraphRecord(g);

            IGraphRestore igr2 = g.CreateRestorePoint();

            g.AddNode("", "", "", 0);

            igr2.Restore();

            Assert.AreEqual(igr1, g.Restore);
            Assert.IsFalse(igr2.CanBeRestored());
            Assert.IsTrue(igr1.CanBeRestored());

            Assert.IsTrue(gr2.Compare(g));

            IGraphRestore igr3 = g.CreateRestorePoint();

            g.AddNode("", "", "", 0);

            igr1.Restore();

            Assert.IsTrue(gr1.Compare(g));

            Assert.IsFalse(igr1.CanBeRestored());
            Assert.IsFalse(igr3.CanBeRestored());
            Assert.AreEqual(null, g.Restore);
        }

        // keep a restore but then abandon it, committing to all our changes

        // restore to intermediate point then start a new restore
        {
            Graph g = new Graph(null);

            IGraphRestore igr1 = g.CreateRestorePoint();

            g.AddNode("", "", "", 0);

            IGraphRestore igr2 = g.CreateRestorePoint();

            g.AddNode("", "", "", 0);

            IGraphRestore igr3 = g.CreateRestorePoint();

            g.AddNode("", "", "", 0);

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
        Graph g = new Graph(null);

        Assert.IsTrue(g.Bounds().Equals(new Box2()));

        INode n1 = g.AddNode("", "", "", 1.0f);

        Assert.IsTrue(g.Bounds().Equals(new Box2(new Vector2(-1, -1), new Vector2(1, 1))));

        INode n2 = g.AddNode("", "", "", 2.0f);

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
    //        Graph g = new Graph(null);

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
    //        Graph g = new Graph(null);

    //        INode n1 = g.AddNode("xx", "yy", "zz", 1.0);
    //        INode n2 = g.AddNode("aa", "bb", "cc", 1.0);
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

    private void TestCatchArgument(Action action)
    {
        bool thrown = false;

        try
        {
            action.Invoke();
        }
        catch (ArgumentException)
        {
            thrown = true;
        }

        Assert.IsTrue(thrown);
    }

    [Test]
    public void TestConnect_Exceptions()
    {
        Graph g = new Graph(null);

        // cannot Connect two nodes we never neard of
        TestCatchArgument(() => g.Connect(new Node("", "", "", 0),
              new Node("", "", "", 0), 0, 0, 0));

        INode n = g.AddNode("n", "x", "y", 10);

        // cannot Connect a node we know and one we don't
        TestCatchArgument(() => g.Connect(n, new Node("", "", "", 0), 0, 0, 0));
    }
}
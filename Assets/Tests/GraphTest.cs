using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Generation.G;

public class GraphTest
{
    [Test]
    public void TestAddNode()
    {
        Graph g = new Graph();

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
        Graph g = new Graph();

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
        Graph g = new Graph();

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
        Graph g = new Graph();

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
        Graph g = new Graph();
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
        Graph g = new Graph();

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

        GraphRecord(Graph g)
        {
            m_nodes = new HashSet<INode>(g.GetAllNodes());
            m_edges = new HashSet<DirectedEdge>(g.GetAllEdges());

            foreach (INode n in m_nodes)
            {
                m_positions.Add(n, n.Position);
            }
        }

        bool Compare(Graph g)
        {
            if (m_nodes.Count != g.NumNodes())
                return false;

            if (m_nodes != new HashSet<INode>(g.GetAllNodes()))
                return false;

            if (m_edges.Count != g.NumEdges())
                return false;

            if (m_edges != new HashSet<DirectedEdge>(g.GetAllEdges()))
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

    //@Test
    //   public void testGraphRecord() throws Exception
    //{
    //      // same if empty
    //      {
    //        Graph g = new Graph();

    //        GraphRecord gr = new GraphRecord(g);

    //        assertTrue(gr.Compare(g));
    //    }

    //      // same if one node
    //    {
    //        Graph g = new Graph();

    //        g.addNode("", "", "", 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        assertTrue(gr.Compare(g));
    //    }

    //      // same if two nodes and an edge
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);

    //        INode n2 = g.addNode("", "", "", 0);

    //        g.connect(n1, n2, 0, 0, 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        assertTrue(gr.Compare(g));
    //    }

    //      // same if node added and removed
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr = new GraphRecord(g);

    //        INode n1 = g.addNode("", "", "", 0);
    //        g.removeNode(n1);

    //        assertTrue(gr.Compare(g));
    //    }

    //      // same if edge added and removed
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);

    //        INode n2 = g.addNode("", "", "", 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        g.connect(n1, n2, 0, 0, 0);

    //        g.disconnect(n1, n2);

    //        assertTrue(gr.Compare(g));
    //    }

    //      // different if node added
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr = new GraphRecord(g);

    //        g.addNode("", "", "", 0);

    //        assertFalse(gr.Compare(g));
    //    }

    //      // different if edge added
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);

    //        INode n2 = g.addNode("", "", "", 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        g.connect(n1, n2, 0, 0, 0);

    //        assertFalse(gr.Compare(g));
    //    }

    //      // different if node moved
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        n1.setPos(new Vector2(1, 0));

    //        assertFalse(gr.Compare(g));
    //    }
    //}

    //@Test
    //   public void testCreateRestorePoint() throws Exception
    //{
    //      // nop
    //      {
    //        Graph g = new Graph();

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        assertTrue(igr.Restore());
    //        // restores only work once...
    //        assertFalse(igr.Restore());

    //        assertTrue(gr.Compare(g));
    //    }

    //      // add node to empty
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        g.addNode("", "", "", 0);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // add nodes and edge to empty
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);

    //        g.connect(n1, n2, 0, 0, 0);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // get back removed nodes and edges
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);

    //        g.connect(n1, n2, 0, 0, 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        g.disconnect(n1, n2);

    //        g.removeNode(n1);
    //        g.removeNode(n2);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // add and remove a node shouldn't break anything
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        INode n1 = g.addNode("", "", "", 0);

    //        g.removeNode(n1);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // add and remove an edge shouldn't break anything
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        g.connect(n1, n2, 0, 0, 0);
    //        g.disconnect(n1, n2);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // add and remove and re-add an edge shouldn't break anything
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        g.connect(n1, n2, 0, 0, 0);
    //        g.disconnect(n1, n2);
    //        g.connect(n1, n2, 0, 0, 0);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // remove and add an edge shouldn't break anything
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);
    //        g.connect(n1, n2, 0, 0, 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        g.disconnect(n1, n2);
    //        g.connect(n1, n2, 0, 0, 0);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // remove and add and re-remove an edge shouldn't break anything
    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);
    //        g.connect(n1, n2, 0, 0, 0);

    //        GraphRecord gr = new GraphRecord(g);

    //        IGraphRestore igr = g.createRestorePoint();

    //        g.disconnect(n1, n2);
    //        g.connect(n1, n2, 0, 0, 0);
    //        g.disconnect(n1, n2);

    //        igr.Restore();

    //        assertTrue(gr.Compare(g));
    //    }

    //      // multiple restore, unchained
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr1 = new GraphRecord(g);

    //        IGraphRestore igr1 = g.createRestorePoint();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);

    //        g.connect(n1, n2, 0, 0, 0);

    //        GraphRecord gr2 = new GraphRecord(g);

    //        IGraphRestore igr2 = g.createRestorePoint();

    //        INode n3 = g.addNode("", "", "", 0);
    //        INode n4 = g.addNode("", "", "", 0);

    //        g.connect(n3, n4, 0, 0, 0);

    //        assertEquals(igr2, g.currentRestore());

    //        igr2.Restore();

    //        assertFalse(igr2.CanBeRestored());
    //        assertTrue(igr1.CanBeRestored());
    //        assertEquals(igr1, g.currentRestore());

    //        assertTrue(gr2.Compare(g));

    //        igr1.Restore();
    //        assertFalse(igr1.CanBeRestored());

    //        assertTrue(gr1.Compare(g));
    //    }

    //      // chained restore
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr1 = new GraphRecord(g);

    //        IGraphRestore igr1 = g.createRestorePoint();

    //        INode n1 = g.addNode("", "", "", 0);
    //        INode n2 = g.addNode("", "", "", 0);

    //        g.connect(n1, n2, 0, 0, 0);

    //        IGraphRestore igr2 = g.createRestorePoint();

    //        INode n3 = g.addNode("", "", "", 0);
    //        INode n4 = g.addNode("", "", "", 0);

    //        g.connect(n3, n4, 0, 0, 0);

    //        igr1.Restore();
    //        assertFalse(igr2.CanBeRestored());
    //        assertEquals(null, g.currentRestore());

    //        assertTrue(gr1.Compare(g));
    //    }

    //      // restore to intermediate point then start a new restore
    //    {
    //        Graph g = new Graph();

    //        GraphRecord gr1 = new GraphRecord(g);

    //        IGraphRestore igr1 = g.createRestorePoint();

    //        g.addNode("", "", "", 0);

    //        GraphRecord gr2 = new GraphRecord(g);

    //        IGraphRestore igr2 = g.createRestorePoint();

    //        g.addNode("", "", "", 0);

    //        igr2.Restore();

    //        assertEquals(igr1, g.currentRestore());
    //        assertFalse(igr2.CanBeRestored());
    //        assertTrue(igr1.CanBeRestored());

    //        assertTrue(gr2.Compare(g));

    //        IGraphRestore igr3 = g.createRestorePoint();

    //        g.addNode("", "", "", 0);

    //        igr1.Restore();

    //        assertTrue(gr1.Compare(g));

    //        assertFalse(igr1.CanBeRestored());
    //        assertFalse(igr3.CanBeRestored());
    //        assertEquals(null, g.currentRestore());
    //    }

    //      // keep a restore but then abandon it, committing to all our changes

    //    // restore to intermediate point then start a new restore
    //    {
    //        Graph g = new Graph();

    //        IGraphRestore igr1 = g.createRestorePoint();

    //        g.addNode("", "", "", 0);

    //        IGraphRestore igr2 = g.createRestorePoint();

    //        g.addNode("", "", "", 0);

    //        IGraphRestore igr3 = g.createRestorePoint();

    //        g.addNode("", "", "", 0);

    //        GraphRecord gr1 = new GraphRecord(g);

    //        g.clearRestore();

    //        // should still have all our changes
    //        assertTrue(gr1.Compare(g));
    //        // and all the restores shoudl know they are now invalid
    //        assertFalse(igr1.CanBeRestored());
    //        assertFalse(igr2.CanBeRestored());
    //        assertFalse(igr3.CanBeRestored());
    //    }
    //}

    //@Test
    //   public void testXYBounds() throws Exception
    //{
    //    Graph g = new Graph();

    //assertTrue(g.bounds().equals(new Box()));

    //INode n1 = g.addNode("", "", "", 1.0);

    //assertTrue(g.bounds().equals(new Box(new Vector2(-1, -1), new Vector2(1, 1))));

    //INode n2 = g.addNode("", "", "", 2.0);

    //assertTrue(g.bounds().equals(new Box(new Vector2(-2, -2), new Vector2(2, 2))));

    //n1.setPos(new Vector2(-2, 0));

    //assertTrue(g.bounds().equals(new Box(new Vector2(-3, -2), new Vector2(2, 2))));

    //n2.setPos(new Vector2(10, 10));

    //assertTrue(g.bounds().equals(new Box(new Vector2(-3, -1), new Vector2(12, 12))));
    //   }

    //   @Test
    //   public void testPrint()
    //{
    //    {
    //        Graph g = new Graph();

    //        g.addNode("xx", "yy", "zz", 1.0);
    //        g.addNode("aa", "bb", "cc", 1.0);

    //        String s = g.print();

    //        assertTrue(s.contains("xx"));
    //        assertTrue(s.contains("yy"));
    //        assertTrue(s.contains("zz"));
    //        assertTrue(s.contains("aa"));
    //        assertTrue(s.contains("bb"));
    //        assertTrue(s.contains("cc"));
    //        assertTrue(s.contains("{"));
    //        assertTrue(s.contains("}"));
    //    }

    //    {
    //        Graph g = new Graph();

    //        INode n1 = g.addNode("xx", "yy", "zz", 1.0);
    //        INode n2 = g.addNode("aa", "bb", "cc", 1.0);
    //        g.connect(n1, n2, 0, 0, 0);

    //        String s = g.print();

    //        String splits[] = s.split("[\\{\\}]");

    //        assertEquals(5, splits.length);

    //        // after the last closing "}" wer have only a linefeed
    //        assertEquals("", splits[4].trim());

    //        // the two nodes can come out in any order
    //        int first = splits[0].contains("aa") ? 0 : 2;
    //        int second = 2 - first;

    //        assertTrue(splits[first].contains("aa"));
    //        assertTrue(splits[second].contains("xx"));

    //        // each node should be followed by a connects block that mentions the other
    //        assertTrue(splits[first + 1].contains("xx"));
    //        assertTrue(splits[second + 1].contains("aa"));
    //    }
    //}

    //private void testCatchUnsupported(Runnable action)
    //{
    //    boolean thrown = false;

    //    try
    //    {
    //        action.run();
    //    }
    //    catch (UnsupportedOperationException uoe)
    //    {
    //        thrown = true;
    //    }

    //    assertTrue(thrown);
    //}

    //@Test
    //   public void testConnect_Exceptions()
    //{
    //    Graph g = new Graph();

    //    // cannot connect two nodes we never neard of
    //    testCatchUnsupported(()->g.connect(new Node("", "", "", 0),
    //          new Node("", "", "", 0), 0, 0, 0));

    //    INode n = g.addNode("n", "x", "y", 10);

    //    // cannot connect a node we know and one we don't
    //    testCatchUnsupported(()->g.connect(n, new Node("", "", "", 0), 0, 0, 0));
    //}
}

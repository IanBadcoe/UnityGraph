using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Generation.G;
using System;
using Assets.Generation.U;
using Assets.Generation.G.GLInterfaces;
using Assets.Generation.Templates;

public class TemplateTest
{
    [Test]
    public void TestExpand_Positioning()
    {
        // no ins, outs or connections, just swapping one disconnected node for another...
        // place directly on replaced node
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.Internal, "a");

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("x", "", "", 0);
            a.Position = new Vector2(-4, 3);

            Assert.IsTrue(t.Expand(g, a, new ClRand(1)));
            Assert.AreEqual(1, g.NumNodes());
            INode new_n = g.GetAllNodes()[0];
            Assert.AreEqual("a", new_n.Name);
            Assert.AreEqual(new Vector2(-4, 3), new_n.Position);
        }

        // no ins, outs or connections, just swapping one disconnected node for another...
        // place offset from replaced node
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.Internal, "a",
                  true, "<target>", null, null,
                  "", 0.0f);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode n = g.AddNode("x", "", "", 0);
            n.Position = new Vector2(-4, 3);

            Assert.IsTrue(t.Expand(g, n, new ClRand(1)));
            Assert.AreEqual(1, g.NumNodes());
            INode new_n = g.GetAllNodes()[0];
            Assert.AreEqual("a", new_n.Name);

            // we offset by 5 in a ClRand direction
            float dist = (new Vector2(-4, 3) - new_n.Position).magnitude;
            Assert.AreEqual(5, dist, 1e-6);
        }

        // an "in" and a replaced node,
        // place new node on the "in"
        // (poss not a desirable scenario but should work...)
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.In, "in");
            tb.AddNode(NodeRecord.NodeType.Internal, "a",
                  false, "in", null, null,
                  "", 0.0f);
            tb.Connect("in", "a", 0, 0, 0);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("x", "", "", 0);
            a.Position = new Vector2(-4, 3);
            INode in_n = g.AddNode("in", "", "", 0);
            in_n.Position = new Vector2(10, 9);

            g.Connect(in_n, a, 0, 0, 0);

            Assert.IsTrue(t.Expand(g, a, new ClRand(1)));
            Assert.AreEqual(2, g.NumNodes());
            Assert.IsNotNull(FindNode(g, "in"));
            INode new_n = FindNode(g, "a");
            Assert.IsNotNull(new_n != null);
            Assert.AreEqual(new Vector2(10, 9), new_n.Position);
        }

      // an "in" and a replaced node,
      // place new node offset from the "in"
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.In, "in");
            tb.AddNode(NodeRecord.NodeType.Internal, "a",
                  true, "in", null, null,
                  "", 0.0f);
            tb.Connect("in", "a", 0, 0, 0);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("x", "", "", 0);
            a.Position = new Vector2(-4, 3);
            INode in_n = g.AddNode("in", "", "", 0);
            in_n.Position = new Vector2(10, 9);

            g.Connect(in_n, a, 0, 0, 0);

            Assert.IsTrue(t.Expand(g, a, new ClRand(1)));
            Assert.AreEqual(2, g.NumNodes());
            Assert.IsNotNull(FindNode(g, "in"));
            INode new_n = FindNode(g, "a");
            Assert.IsNotNull(new_n != null);

            // we offset by 5 in a ClRand direction
            float dist = (new Vector2(10, 9) - new_n.Position).magnitude;
            Assert.AreEqual(5, dist, 1e-6);
        }

      // an "in" and a replaced node,
      // place new node on replaced node but moved towards "in"
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.In, "in");
            tb.AddNode(NodeRecord.NodeType.Internal, "a",
                  false, "<target>", "in", null,
                  "", 0.0f);
            tb.Connect("in", "a", 0, 0, 0);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("x", "", "", 0);
            a.Position = new Vector2(-4, 3);
            INode in_n = g.AddNode("in", "", "", 0);
            in_n.Position = new Vector2(-14, -7);

            g.Connect(in_n, a, 0, 0, 0);

            Assert.IsTrue(t.Expand(g, a, new ClRand(1)));
            Assert.AreEqual(2, g.NumNodes());
            Assert.IsNotNull(FindNode(g, "in"));
            INode new_n = FindNode(g, "a");
            Assert.IsNotNull(new_n != null);

            // position on replaced node but 10% of the way towards "in"
            Assert.AreEqual(new Vector2(-5, 2), new_n.Position);
        }

      // an "in" and a replaced node,
      // place new node on replaced node but moved away from  "in"
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.In, "in");
            tb.AddNode(NodeRecord.NodeType.Internal, "a",
                  false, "<target>", null, "in",
                  "", 0.0f);
            tb.Connect("in", "a", 0, 0, 0);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("x", "", "", 0);
            a.Position = new Vector2(-4, 3);
            INode in_n = g.AddNode("in", "", "", 0);
            in_n.Position = new Vector2(-14, -7);

            g.Connect(in_n, a, 0, 0, 0);

            Assert.IsTrue(t.Expand(g, a, new ClRand(1)));
            Assert.AreEqual(2, g.NumNodes());
            Assert.IsNotNull(FindNode(g, "in"));
            INode new_n = FindNode(g, "a");
            Assert.IsNotNull(new_n != null);

            // position on replaced node but 10% of the way towards "in"
            Assert.AreEqual(new Vector2(-3, 4), new_n.Position);
        }

      // an "in", an "out", and a replaced node,
      // place new node on replaced node but moved away from "in"
      // and towards "out"
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.In, "in");
            tb.AddNode(NodeRecord.NodeType.Out, "out");
            tb.AddNode(NodeRecord.NodeType.Internal, "a",
                  false, "<target>", "out", "in",
                  "", 0.0f);
            tb.Connect("in", "a", 0, 0, 0);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("x", "", "", 0);
            a.Position = new Vector2(10, 10);
            INode in_n = g.AddNode("in", "", "", 0);
            in_n.Position = new Vector2(20, 10);
            INode out_n = g.AddNode("out", "", "", 0);
            out_n.Position = new Vector2(10, 20);

            g.Connect(in_n, a, 0, 0, 0);
            g.Connect(a, out_n, 0, 0, 0);

            Assert.IsTrue(t.Expand(g, a, new ClRand(1)));
            Assert.AreEqual(3, g.NumNodes());
            Assert.IsNotNull(FindNode(g, "in"));
            INode new_n = FindNode(g, "a");
            Assert.IsNotNull(new_n != null);

            // position on replaced node but 10% of the way away from "in"
            // and 10% (rel to original position) towards out
            Assert.AreEqual(new Vector2(9, 11), new_n.Position);
        }
    }

    //    class PostExpandReporter implements Template.IPostExpand
    //    {
    //        @Override
    //      public void AfterExpand(INode n)
    //    {
    //        AfterExpandCount++;
    //        AfterExpandAfterDone = DoneCount > 0;
    //    }

    //    @Override
    //      public void Done()
    //    {
    //        DoneCount++;
    //        DoneAfterAfterExpand = AfterExpandCount > 0;
    //    }

    //    int AfterExpandCount = 0;
    //    int DoneCount = 0;
    //    bool DoneAfterAfterExpand = false;
    //    bool AfterExpandAfterDone = false;
    //}

    //[Test]
    //   public void testPostExpand()
    //{
    //    {
    //        PostExpandReporter ipo = new PostExpandReporter();

    //        TemplateBuilder tb = new TemplateBuilder("", "", ipo);

    //        tb.AddNode(NodeRecord.NodeType.Internal, "a");

    //        Template t = tb.Build();

    //        Graph g = new Graph(null);

    //        INode a = g.AddNode("x", "", "", 0);

    //        Assert.IsTrue(t.Expand(g, a, new ClRand(1)));

    //        Assert.AreEqual(1, ipo.AfterExpandCount);
    //        Assert.AreEqual(1, ipo.DoneCount);
    //        Assert.IsTrue(ipo.DoneAfterAfterExpand);
    //        Assert.IsFalse(ipo.AfterExpandAfterDone);
    //    }
    //}

    [Test]
    public void TestExpand_Fails()
    {
        // cannot expand with unavoidable crossing edges
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.In, "in");
            tb.AddNode(NodeRecord.NodeType.Internal, "x",
                  false, "<target>", null, null,
                  "", 0);

            tb.Connect("in", "x", 0, 0, 0);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("a", "", "", 0);
            INode b = g.AddNode("b", "", "", 0);
            INode c = g.AddNode("c", "", "", 0);
            INode d = g.AddNode("d", "", "", 0);

            a.Position = new Vector2(10, 0);
            b.Position = new Vector2(-10, 0);
            c.Position = new Vector2(0, 10);
            d.Position = new Vector2(0, -10);

            g.Connect(a, b, 0, 0, 0);
            g.Connect(c, d, 0, 0, 0);

            IGraphRestore igr = g.CreateRestorePoint();

            // cannot succeed as want to re-connect c and d
            // but that line has to hit the a -> b edge
            Assert.IsFalse(t.Expand(g, d, new ClRand(1)));

            // failed template expansion is destructive, so restore our graph
            igr.Restore();

            // just to prove this is why we are failing
            g.Disconnect(a, b);

            Assert.IsTrue(t.Expand(g, d, new ClRand(1)));
        }

        // fail with various wrong numbers of ins/outs
        {
            TemplateBuilder tb = new TemplateBuilder("", "");

            tb.AddNode(NodeRecord.NodeType.In, "in");
            tb.AddNode(NodeRecord.NodeType.Out, "out");
            tb.AddNode(NodeRecord.NodeType.Internal, "x",
                  false, "<target>", null, null,
                  "", 0);

            Template t = tb.Build();

            Graph g = new Graph(null);

            INode a = g.AddNode("a", "", "", 0);
            INode b = g.AddNode("b", "", "", 0);
            INode c = g.AddNode("c", "", "", 0);
            INode d = g.AddNode("d", "", "", 0);
            INode x = g.AddNode("x", "", "", 0);

            {
                IGraphRestore igr = g.CreateRestorePoint();

                // no ins or outs
                Assert.IsFalse(t.Expand(g, x, new ClRand(1)));

                // failed template expansion is destructive, so restore our graph
                igr.Restore();
            }

            g.Connect(a, x, 0, 0, 0);

            {
                IGraphRestore igr = g.CreateRestorePoint();

                // still no outs
                Assert.IsFalse(t.Expand(g, x, new ClRand(1)));

                // failed template expansion is destructive, so restore our graph
                igr.Restore();
            }

            g.Connect(x, b, 0, 0, 0);

            {
                IGraphRestore igr = g.CreateRestorePoint();

                // 1 in 1 out
                Assert.IsTrue(t.Expand(g, x, new ClRand(1)));

                // failed template expansion is destructive, so restore our graph
                igr.Restore();
            }

            g.Connect(c, x, 0, 0, 0);

            {
                IGraphRestore igr = g.CreateRestorePoint();

                // two ins 1 out
                Assert.IsFalse(t.Expand(g, x, new ClRand(1)));

                // failed template expansion is destructive, so restore our graph
                igr.Restore();
            }

            g.Connect(x, d, 0, 0, 0);

            {
                IGraphRestore igr = g.CreateRestorePoint();

                // 2 ins 2 outs
                Assert.IsFalse(t.Expand(g, x, new ClRand(1)));

                // failed template expansion is destructive, so restore our graph
                igr.Restore();
            }

            g.Disconnect(b, x);

            {
                IGraphRestore igr = g.CreateRestorePoint();

                // 0 ins 2 outs
                Assert.IsFalse(t.Expand(g, x, new ClRand(1)));

                // failed template expansion is destructive, so restore our graph
                igr.Restore();
            }

            g.Disconnect(x, c);

            {
                IGraphRestore igr = g.CreateRestorePoint();

                // 0 ins 1 out
                Assert.IsFalse(t.Expand(g, x, new ClRand(1)));

                // failed template expansion is destructive, so restore our graph
                igr.Restore();
            }
        }
    }

    [Test]
    public void TestCodes()
    {
        TemplateBuilder tb = new TemplateBuilder("a", "xyz");
        Template t = tb.Build();

        Assert.AreEqual("xyz", t.Codes);
    }

    [Test]
    public void TestNodesAdded()
    {
        {
            TemplateBuilder tb = new TemplateBuilder("", "");
            Template t = tb.Build();

            Assert.AreEqual(-1, t.NodesAdded());
        }

        {
            TemplateBuilder tb = new TemplateBuilder("", "");
            tb.AddNode(NodeRecord.NodeType.In, "in1");
            tb.AddNode(NodeRecord.NodeType.Out, "out1");
            Template t = tb.Build();

            // in or out nodes have no effect on change in total
            Assert.AreEqual(-1, t.NodesAdded());
        }

        {
            TemplateBuilder tb = new TemplateBuilder("", "");
            tb.AddNode(NodeRecord.NodeType.Internal, "internal",
                  false, "<target>", null, null,
                  "", 0);

            Template t = tb.Build();

            // one replaced node is removed and an internal node is added
            Assert.AreEqual(0, t.NodesAdded());
        }

        {
            TemplateBuilder tb = new TemplateBuilder("", "");
            tb.AddNode(NodeRecord.NodeType.Internal, "internal",
                  false, "<target>", null, null,
                  "", 0);
            tb.AddNode(NodeRecord.NodeType.Internal, "internal2",
                  false, "<target>", null, null,
                  "", 0);

            Template t = tb.Build();

            // one replaced node is removed and two internal nodes are added
            Assert.AreEqual(1, t.NodesAdded());
        }
    }

    // finds first node of required name, unit tests keep this unique
    // but that isn't a requirement of graphs generally
    private static INode FindNode(Graph g, String name)
    {
        foreach (INode n in g.GetAllNodes())
        {
            if (n.Name == name)
                return n;
        }

        return null;
    }
}
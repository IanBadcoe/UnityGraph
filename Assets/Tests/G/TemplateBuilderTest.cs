using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Generation.G;
using System;
using Assets.Generation.U;
using Assets.Generation.Templates;
using Assets.Generation.GeomRep;

public class TemplateBuilderTest
{
    [Test]
    public void TestAddNode()
    {
        {
            TemplateBuilder t = new TemplateBuilder("", "");

            Assert.IsNull(t.FindNodeRecord("n1"));

            t.AddNode(NodeRecord.NodeType.Internal, "n1");

            NodeRecord nr = t.FindNodeRecord("n1");

            Assert.IsNotNull(nr);
            Assert.AreEqual("n1", nr.Name);
            Assert.AreEqual(NodeRecord.NodeType.Internal, nr.Type);
            Assert.AreEqual(false, nr.Nudge);
            // internal nodes always have "PositionOn"
            Assert.IsNotNull(nr.PositionOn);
            Assert.IsNull(nr.PositionTowards);
            Assert.IsNull(nr.PositionAwayFrom);
            Assert.AreEqual(0.0, nr.Radius, 0.0);
            Assert.AreEqual("", nr.Codes);
            Assert.AreEqual(null, nr.Layout);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            Assert.IsNull(t.FindNodeRecord("n1"));

            t.AddNode(NodeRecord.NodeType.In, "n1");

            NodeRecord nr = t.FindNodeRecord("n1");

            Assert.IsNotNull(nr);
            Assert.AreEqual("n1", nr.Name);
            Assert.AreEqual(NodeRecord.NodeType.In, nr.Type);
            Assert.AreEqual(false, nr.Nudge);
            // In nodes have none of these three
            Assert.IsNull(nr.PositionOn);
            Assert.IsNull(nr.PositionTowards);
            Assert.IsNull(nr.PositionAwayFrom);
            Assert.AreEqual(0.0, nr.Radius, 0.0);
            Assert.AreEqual("", nr.Codes);
            Assert.AreEqual(null, nr.Layout);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            Assert.IsNull(t.FindNodeRecord("n1"));

            t.AddNode(NodeRecord.NodeType.Out, "n1");

            NodeRecord nr = t.FindNodeRecord("n1");

            Assert.IsNotNull(nr);
            Assert.AreEqual("n1", nr.Name);
            Assert.AreEqual(NodeRecord.NodeType.Out, nr.Type);
            Assert.AreEqual(false, nr.Nudge);
            // Out nodes have none of these three
            Assert.IsNull(nr.PositionOn);
            Assert.IsNull(nr.PositionTowards);
            Assert.IsNull(nr.PositionAwayFrom);
            Assert.AreEqual(0.0, nr.Radius, 0.0);
            Assert.AreEqual("", nr.Codes);
            Assert.AreEqual(null, nr.Layout);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            t.AddNode(NodeRecord.NodeType.In, "In");
            t.AddNode(NodeRecord.NodeType.Out, "Out");

            t.AddNode(NodeRecord.NodeType.Internal, "n1",
                  true, "In", "Out", null,
                  "xx", 3.0f, 1.0f, CircularGeomLayout.Instance);

            NodeRecord nr = t.FindNodeRecord("n1");

            Assert.IsNotNull(nr);
            Assert.AreEqual("n1", nr.Name);
            Assert.AreEqual(NodeRecord.NodeType.Internal, nr.Type);
            Assert.AreEqual(true, nr.Nudge);
            Assert.IsNotNull(nr.PositionTowards);
            Assert.IsNotNull(nr.PositionOn);
            Assert.IsNull(nr.PositionAwayFrom);
            Assert.AreEqual("In", nr.PositionOn.Name);
            Assert.AreEqual("Out", nr.PositionTowards.Name);
            Assert.AreEqual(3.0, nr.Radius, 0.0);
            Assert.AreEqual(1.0, nr.WallThickness, 0.0);
            Assert.AreEqual("xx", nr.Codes);
            Assert.AreEqual(CircularGeomLayout.Instance, nr.Layout);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            t.AddNode(NodeRecord.NodeType.In, "In");
            t.AddNode(NodeRecord.NodeType.Out, "Out");

            t.AddNode(NodeRecord.NodeType.Internal, "n1",
                  true, "In", null, "Out",
                  "xx", 3.0f, -1, null);

            NodeRecord nr = t.FindNodeRecord("n1");

            Assert.IsNotNull(nr);
            Assert.AreEqual("n1", nr.Name);
            Assert.AreEqual(NodeRecord.NodeType.Internal, nr.Type);
            Assert.AreEqual(true, nr.Nudge);
            Assert.IsNotNull(nr.PositionOn);
            Assert.IsNull(nr.PositionTowards);
            Assert.IsNotNull(nr.PositionAwayFrom);
            Assert.AreEqual("In", nr.PositionOn.Name);
            Assert.AreEqual("Out", nr.PositionAwayFrom.Name);
            Assert.AreEqual(3.0, nr.Radius, 0.0);
            Assert.AreEqual(-1.0, nr.WallThickness, 0.0);
            Assert.AreEqual("xx", nr.Codes);
            Assert.AreEqual(null, nr.Layout);
        }
    }

    [Test]
    public void TestAddNode_Exceptions()
    {
        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, null);
            }
            catch (NullReferenceException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            // we use "->" to separate node names when naming connections
            // therefore do nto permit in node names themselves...
            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, "x->");
            }
            catch (ArgumentException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            // we use "<target>" to mean the node we are replacing, so we can't
            // highjack that
            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, "<target>");
            }
            catch (ArgumentException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            // we use NodeRecord.NodeType.Target to mean the node we are replacing, so we can't
            // create another one
            try
            {
                t.AddNode(NodeRecord.NodeType.Target, "x");
            }
            catch (ArgumentException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            t.AddNode(NodeRecord.NodeType.Internal, "n1");

            bool thrown = false;

            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, "n1");
            }
            catch (TemplateBuilder.DuplicateNodeException dne)
            {
                thrown = true;
                Assert.AreEqual("n1", dne.NodeName);
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, "n1",
                      true, "q", null, null,
                      "", 0, 0, null);
            }
            catch (TemplateBuilder.UnknownNodeException une)
            {
                thrown = true;
                Assert.AreEqual("q", une.NodeName);
                Assert.AreEqual("positionOnName", une.Argument);
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, "n1",
                      true, "<target>", "qq", null,
                      "", 0, 0, null);
            }
            catch (TemplateBuilder.UnknownNodeException une)
            {
                thrown = true;
                Assert.AreEqual("qq", une.NodeName);
                Assert.AreEqual("positionTowardsName", une.Argument);
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, "n1",
                      true, "<target>", null, "qqq",
                      "", 0, 0, null);
            }
            catch (TemplateBuilder.UnknownNodeException une)
            {
                thrown = true;
                Assert.AreEqual("qqq", une.NodeName);
                Assert.AreEqual("positionAwayFromName", une.Argument);
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            try
            {
                t.AddNode(NodeRecord.NodeType.Internal, "x",
                      false, null, null, null,
                      "", 0, 0, null);
            }
            catch (NullReferenceException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }
    }

    [Test]
    public void TestConnect_Exceptions()
    {
        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            try
            {
                t.Connect(null, "x", 0, 0, null);
            }
            catch (NullReferenceException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            try
            {
                t.Connect("x", null, 0, 0, null);
            }
            catch (NullReferenceException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            t.AddNode(NodeRecord.NodeType.In, "y");

            try
            {
                t.Connect("x", "y", 0, 0, null);
            }
            catch (TemplateBuilder.UnknownNodeException une)
            {
                Assert.AreEqual("from", une.Argument);
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            t.AddNode(NodeRecord.NodeType.In, "x");

            try
            {
                t.Connect("x", "y", 0, 0, null);
            }
            catch (TemplateBuilder.UnknownNodeException une)
            {
                Assert.AreEqual("to", une.Argument);
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            t.AddNode(NodeRecord.NodeType.In, "x");

            try
            {
                t.Connect("x", "<target>", 0, 0, null);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            bool thrown = false;

            t.AddNode(NodeRecord.NodeType.In, "x");

            try
            {
                t.Connect("<target>", "x", 0, 0, null);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            TemplateBuilder t = new TemplateBuilder("", "");

            t.AddNode(NodeRecord.NodeType.In, "a");
            t.AddNode(NodeRecord.NodeType.In, "b");
            t.Connect("a", "b", 0, 0, null);

            {
                bool thrown = false;

                try
                {
                    t.Connect("a", "b", 1, 2, null);
                }
                catch (ArgumentException)
                {
                    thrown = true;
                }

                Assert.IsTrue(thrown);
            }

            {
                bool thrown = false;

                try
                {
                    t.Connect("b", "a", 0, 0, null);
                }
                catch (ArgumentException)
                {
                    thrown = true;
                }

                Assert.IsTrue(thrown);
            }
        }
    }

    [Test]
    public void TestConnect()
    {
        {
            TemplateBuilder t = new TemplateBuilder("", "");

            Assert.IsNull(t.FindConnectionRecord("a", "b"));

            t.AddNode(NodeRecord.NodeType.In, "a");
            t.AddNode(NodeRecord.NodeType.Internal, "b");

            NodeRecord nra = t.FindNodeRecord("a");
            NodeRecord nrb = t.FindNodeRecord("b");

            t.Connect("a", "b", 1, 2, null);

            Assert.IsNull(t.FindConnectionRecord("b", "a"));

            ConnectionRecord cr = t.FindConnectionRecord("a", "b");

            Assert.IsNotNull(cr);
            Assert.AreEqual(nra, cr.From);
            Assert.AreEqual(nrb, cr.To);
            Assert.AreEqual(1, cr.MaxLength, 0.0);
            Assert.AreEqual(2, cr.HalfWidth, 0.0);
        }
    }
}
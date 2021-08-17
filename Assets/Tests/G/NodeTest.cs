using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Generation.G;
using System;
using Assets.Generation.U;
using Assets.Generation.GeomRep;

public class NodeTest
{
    [Test]
    public void TestNumConnections()
    {
        Node n1 = new Node("", "", 0);
        Node n2 = new Node("", "", 0);

        Assert.AreEqual(0, n1.NumConnections());

        n1.Connect(n2, 0, 0, 0/*, null*/);

        Assert.AreEqual(1, n1.NumConnections());

        n1.Disconnect(n2);

        Assert.AreEqual(0, n1.NumConnections());
    }

    [Test]
    public void TestConnect_Exception()
    {
        {
            Node n1 = new Node("xxx", "", 0);
            Node n2 = new Node("yyy", "", 0);
            n1.Connect(n2, 0, 0, 0/*, null */);

            bool thrown = false;
            try
            {
                n1.Connect(n2, 0, 0, 0/*, null */);
            }
            catch (ArgumentException iae)
            {
                thrown = true;

                // message should mention both nodes
                Assert.IsTrue(iae.Message.Contains("xxx"));
                Assert.IsTrue(iae.Message.Contains("yyy"));
            }

            Assert.IsTrue(thrown);
        }

        {
            Node n1 = new Node("xxx", "", 0);
            Node n2 = new Node("yyy", "", 0);
            n1.Connect(n2, 0, 0, 0/*, null*/);

            bool thrown = false;
            try
            {
                n2.Connect(n1, 0, 0, 0/*, null*/);
            }
            catch (ArgumentException iae)
            {
                thrown = true;

                // message shoulkd mention both nodes
                Assert.IsTrue(iae.Message.Contains("xxx"));
                Assert.IsTrue(iae.Message.Contains("yyy"));
            }

            Assert.IsTrue(thrown);
        }
    }

    [Test]
    public void TestGetConnectionTo()
    {
        {
            Node n1 = new Node("xxx", "", 0);
            Node n2 = new Node("yyy", "", 0);
            n1.Connect(n2, 0, 0, 0/*, null*/);

            DirectedEdge de = n1.GetConnectionTo(n2);

            Assert.NotNull(de);
            Assert.AreEqual(de.Start, n1);
            Assert.AreEqual(de.End, n2);
        }

        {
            Node n1 = new Node("xxx", "", 0);
            Node n2 = new Node("yyy", "", 0);
            n1.Connect(n2, 0, 0, 0/*, null*/);

            DirectedEdge de = n1.GetConnectionTo(null);

            Assert.Null(de);

            // these are found by node-identity, not name
            de = n1.GetConnectionTo(new Node("yyy", "", 0));

            Assert.Null(de);
        }
    }

    [Test]
    public void TestGetConnectionFrom()
    {
        {
            Node n1 = new Node("xxx", "", 0);
            Node n2 = new Node("yyy", "", 0);
            n1.Connect(n2, 0, 0, 0/*, null*/);

            DirectedEdge de = n2.GetConnectionFrom(n1);

            Assert.NotNull(de);
            Assert.AreEqual(de.Start, n1);
            Assert.AreEqual(de.End, n2);
        }

        {
            Node n1 = new Node("xxx", "", 0);
            Node n2 = new Node("yyy", "", 0);
            n1.Connect(n2, 0, 0, 0/*, null*/);

            DirectedEdge de = n1.GetConnectionFrom(null);

            Assert.Null(de);

            // these are found by node-identity, not name
            de = n1.GetConnectionFrom(new Node("xxx", "", 0));

            Assert.Null(de);
        }
    }

    [Test]
    public void TestConnects()
    {
        Node n1 = new Node("", "", 0);
        Node n2 = new Node("", "", 0);
        Node n3 = new Node("", "", 0);

        n1.Connect(n2, 1, 2, 3/*, null*/);

        Assert.IsTrue(n1.Connects(n2));
        Assert.IsTrue(n2.Connects(n1));
        Assert.False(n1.Connects(n3));
        Assert.False(n3.Connects(n1));
        Assert.False(n2.Connects(n3));
        Assert.False(n3.Connects(n2));

        Assert.IsTrue(n1.ConnectsForwards(n2));
        Assert.False(n2.ConnectsForwards(n1));
        Assert.False(n1.ConnectsBackwards(n2));
        Assert.IsTrue(n2.ConnectsBackwards(n1));
    }

    //[Test]
    //public void testSetName()
    //{
    //    Node n1 = new Node("", 0);

    //    Assert.AreEqual("", n1.Name);

    //    n1.Name = "x";
    //    Assert.AreEqual("x", n1.getName());

    //    bool thrown = false;

    //    try
    //    {
    //        n1.setName(null);
    //    }
    //    catch (NullReferenceException npe)
    //    {
    //        thrown = true;
    //    }

    //    Assert.IsTrue(thrown);
    //}

    [Test]
    public void TestGeomLayout()
    {
        Node n1 = new Node("", "", 0);

        Assert.IsNull(n1.Layout);

        Node n2 = new Node("", "", 0, 0, CircularGeomLayout.Instance);

        Assert.AreEqual(CircularGeomLayout.Instance, n2.Layout);
    }
}
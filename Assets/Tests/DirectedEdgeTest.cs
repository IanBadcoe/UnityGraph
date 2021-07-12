using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Generation.G;

public class DirectedEdgeTest
{
    //// A Test behaves as an ordinary method
    //[Test]
    //public void DirectedEdgeTestSimplePasses()
    //{
    //    // Use the Assert class to test conditions
    //}

    //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    //// `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator DirectedEdgeTestWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}

    [Test]
    public void TestHashCode()
    {
        {
            INode n1 = new Node("a", "1", "a1", 0);
            INode n2 = new Node("b", "2", "a2", 1);

            DirectedEdge e1 = new DirectedEdge(n1, n2, 0, 0, 0);
            DirectedEdge e2 = new DirectedEdge(n1, n2, 1, 2, 3);

            // only node-identity affects the hash
            Assert.AreEqual(e1.GetHashCode(), e2.GetHashCode());
        }

        {
            INode n1 = new Node("a", "1", "a1", 0);
            INode n2 = new Node("b", "2", "a2", 1);
            INode n3 = new Node("c", "3", "a3", 2);

            DirectedEdge e1 = new DirectedEdge(n1, n2, 0, 0, 0);
            DirectedEdge e2 = new DirectedEdge(n1, n3, 0, 0, 0);
            DirectedEdge e3 = new DirectedEdge(n3, n2, 0, 0, 0);

            // any node-identity affects the hash
            Assert.AreNotEqual(e1.GetHashCode(), e2.GetHashCode());
            Assert.AreNotEqual(e1.GetHashCode(), e3.GetHashCode());
        }

        {
            INode n1 = new Node("a", "1", "a1", 0);
            INode n2 = new Node("b", "2", "a2", 1);

            DirectedEdge e1 = new DirectedEdge(n1, n2, 0, 0, 0);
            DirectedEdge e2 = new DirectedEdge(n2, n1, 0, 0, 0);

            // asymmetric
            Assert.AreNotEqual(e1.GetHashCode(), e2.GetHashCode());
        }
    }

    [Test]
    public void TestEquals()
    {
        INode n1 = new Node("a", "1", "a1", 0);
        INode n2 = new Node("b", "2", "a2", 1);
        INode n3 = new Node("a", "1", "a1", 0);
        INode n4 = new Node("b", "2", "a2", 1);

        DirectedEdge e1 = new DirectedEdge(n1, n2, 0, 0, 0);

        // only the node identities affect edge identity
        //noinspection objectEqualsNull
        Assert.IsFalse(e1.Equals(null));
        //noinspection EqualsBetweenInconvertibleTypes
        Assert.IsFalse(e1.Equals(1));
        Assert.IsTrue(e1.Equals(new DirectedEdge(n1, n2, 0, 0, 0)));
        Assert.IsTrue(e1.Equals(new DirectedEdge(n1, n2, 1, 1, 1)));
        Assert.IsFalse(e1.Equals(new DirectedEdge(n2, n1, 1, 1, 1)));
        Assert.IsFalse(e1.Equals(new DirectedEdge(n1, n4, 1, 1, 1)));
        Assert.IsFalse(e1.Equals(new DirectedEdge(n3, n2, 1, 1, 1)));
    }

    [Test]
    public void TestOtherNode()
    {
        INode n1 = new Node("a", "1", "a1", 0);
        INode n2 = new Node("b", "2", "a2", 1);
        INode n3 = new Node("a", "1", "a1", 0);

        DirectedEdge e1 = new DirectedEdge(n1, n2, 0, 0, 0);

        // only the node (reference) identities affect edge identity
        Assert.IsNull(e1.OtherNode(null));
        Assert.IsNull(e1.OtherNode(n3));
        Assert.AreEqual(n2, e1.OtherNode(n1));
        Assert.AreEqual(n1, e1.OtherNode(n2));
    }

    [Test]
    public void TestLength()
    {
        INode n1 = new Node("a", "1", "a1", 0);
        INode n2 = new Node("b", "2", "a2", 1);

        DirectedEdge e1 = new DirectedEdge(n1, n2, 0, 0, 0);

        Assert.AreEqual(0, e1.Length(), 0);
        n1.Position = new Vector2(1, 0);
        Assert.AreEqual(1, e1.Length(), 0);
        n2.Position = new Vector2(0, 1);
        Assert.AreEqual(Mathf.Sqrt(2), e1.Length(), 0);
    }

    [Test]
    public void TestConnects()
    {
        INode n1 = new Node("a", "1", "a1", 0);
        INode n2 = new Node("b", "2", "a2", 1);
        INode n3 = new Node("a", "1", "a1", 0);

        DirectedEdge e1 = new DirectedEdge(n1, n2, 0, 0, 0);

        Assert.IsTrue(e1.Connects(n1));
        Assert.IsTrue(e1.Connects(n2));
        Assert.IsFalse(e1.Connects(n3));
    }
}

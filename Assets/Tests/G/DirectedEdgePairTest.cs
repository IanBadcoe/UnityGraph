using Assets.Generation.G;
using NUnit.Framework;

public class DirectedEdgePairTest
{
    [Test]
    public void TestHashCode()
    {
        Node n1 = new Node("", "", 0);
        Node n2 = new Node("", "", 0);
        Node n3 = new Node("", "", 0);
        Node n4 = new Node("", "", 0);
        Node n5 = new Node("", "", 0);

        DirectedEdge de1 = new DirectedEdge(n1, n2, 0, 0, 0);
        DirectedEdge de1a = new DirectedEdge(n1, n2, 1, 2, 3);
        DirectedEdge de2 = new DirectedEdge(n3, n4, 0, 0, 0);
        DirectedEdge de2a = new DirectedEdge(n3, n4, 3, 2, 1);
        DirectedEdge de3 = new DirectedEdge(n1, n5, 0, 0, 0);
        DirectedEdge de4 = new DirectedEdge(n5, n2, 0, 0, 0);
        DirectedEdge de5 = new DirectedEdge(n3, n5, 0, 0, 0);
        DirectedEdge de6 = new DirectedEdge(n5, n4, 0, 0, 0);

        DirectedEdgePair dep1 = new DirectedEdgePair(de1, de2);
        DirectedEdgePair dep1a = new DirectedEdgePair(de1a, de2a);
        DirectedEdgePair dep1b = new DirectedEdgePair(de2, de1);

        // only the edge hashes contribute to the overall hash
        // and those should only care about node identities
        Assert.AreEqual(dep1.GetHashCode(), dep1a.GetHashCode());

        // we don't care about the edge order
        Assert.AreEqual(dep1.GetHashCode(), dep1b.GetHashCode());

        DirectedEdgePair dep2 = new DirectedEdgePair(de3, de2);
        DirectedEdgePair dep3 = new DirectedEdgePair(de4, de2);
        DirectedEdgePair dep4 = new DirectedEdgePair(de1, de5);
        DirectedEdgePair dep5 = new DirectedEdgePair(de1, de6);

        // and any one node changing should change the DEP
        Assert.AreNotEqual(dep1.GetHashCode(), dep2.GetHashCode());
        Assert.AreNotEqual(dep1.GetHashCode(), dep3.GetHashCode());
        Assert.AreNotEqual(dep1.GetHashCode(), dep4.GetHashCode());
        Assert.AreNotEqual(dep1.GetHashCode(), dep5.GetHashCode());
    }

    [Test]
    public void TestEquals()
    {
        Node n1 = new Node("", "", 0);
        Node n2 = new Node("", "", 0);
        Node n3 = new Node("", "", 0);
        Node n4 = new Node("", "", 0);
        Node n5 = new Node("", "", 0);

        DirectedEdge de1 = new DirectedEdge(n1, n2, 0, 0, 0);
        DirectedEdge de1a = new DirectedEdge(n1, n2, 1, 2, 3);
        DirectedEdge de2 = new DirectedEdge(n3, n4, 0, 0, 0);
        DirectedEdge de2a = new DirectedEdge(n3, n4, 3, 2, 1);
        DirectedEdge de3 = new DirectedEdge(n1, n5, 0, 0, 0);
        DirectedEdge de4 = new DirectedEdge(n5, n2, 0, 0, 0);
        DirectedEdge de5 = new DirectedEdge(n3, n5, 0, 0, 0);
        DirectedEdge de6 = new DirectedEdge(n5, n4, 0, 0, 0);

        DirectedEdgePair dep1 = new DirectedEdgePair(de1, de2);
        DirectedEdgePair dep1a = new DirectedEdgePair(de1a, de2a);
        DirectedEdgePair dep1b = new DirectedEdgePair(de2, de1);

        // only the edge hashes contribute to the overall hash
        // and those should only care about node identities
        Assert.IsTrue(dep1.Equals(dep1a));

        // we don't care about the edge order
        Assert.IsTrue(dep1.Equals(dep1b));

        DirectedEdgePair dep2 = new DirectedEdgePair(de3, de2);
        DirectedEdgePair dep3 = new DirectedEdgePair(de4, de2);
        DirectedEdgePair dep4 = new DirectedEdgePair(de1, de5);
        DirectedEdgePair dep5 = new DirectedEdgePair(de1, de6);

        // and any one node changing should change the DEP identity
        Assert.IsFalse(dep1.Equals(dep2));
        Assert.IsFalse(dep1.Equals(dep3));
        Assert.IsFalse(dep1.Equals(dep4));
        Assert.IsFalse(dep1.Equals(dep5));

        Assert.IsFalse(dep1.Equals(1));
    }
}

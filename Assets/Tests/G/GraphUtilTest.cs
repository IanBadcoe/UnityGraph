using Assets.Generation.G;
using Assets.Generation.U;
using NUnit.Framework;
using UnityEngine;

public class GraphUtilTest
{
    [Test]
    public void TestEdgeIntersectSimple_Node()
    {
        Assert.IsNotNull(GraphUtil.EdgeIntersect(MakeNodeAt(1, 0), MakeNodeAt(-1, 0), MakeNodeAt(0, 1), MakeNodeAt(0, -1)));
        Assert.IsNull(GraphUtil.EdgeIntersect(MakeNodeAt(1, 0), MakeNodeAt(-1, 0), MakeNodeAt(1, 0), MakeNodeAt(-1, 0)));

        Assert.IsNotNull(GraphUtil.EdgeIntersect(MakeNodeAt(-1, 0), MakeNodeAt(1, 0), MakeNodeAt(0, -1), MakeNodeAt(0, 1)));
        Assert.IsNull(GraphUtil.EdgeIntersect(MakeNodeAt(-1, 0), MakeNodeAt(1, 0), MakeNodeAt(-1, 0), MakeNodeAt(1, 0)));

        Assert.IsNotNull(GraphUtil.EdgeIntersect(MakeNodeAt(0, 1), MakeNodeAt(0, -1), MakeNodeAt(1, 0), MakeNodeAt(-1, 0)));
        Assert.IsNull(GraphUtil.EdgeIntersect(MakeNodeAt(0, 1), MakeNodeAt(0, -1), MakeNodeAt(0, 1), MakeNodeAt(0, -1)));

        Assert.IsNotNull(GraphUtil.EdgeIntersect(MakeNodeAt(0, -1), MakeNodeAt(0, 1), MakeNodeAt(-1, 0), MakeNodeAt(1, 0)));
        Assert.IsNull(GraphUtil.EdgeIntersect(MakeNodeAt(0, -1), MakeNodeAt(0, 1), MakeNodeAt(0, -1), MakeNodeAt(0, 1)));
    }

    [Test]
    public void TestEdgeIntersectAdjoining_Node()
    {
        // we can detect end-collisions of edges
        Assert.IsNotNull(Util.EdgeIntersect(new Vector2(1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(0, 1)));
        Assert.IsNotNull(Util.EdgeIntersect(new Vector2(-1, 0), new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1)));
        Assert.IsNotNull(Util.EdgeIntersect(new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(-1, 0)));
        Assert.IsNotNull(Util.EdgeIntersect(new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0)));

        Node n1 = MakeNodeAt(1, 0);
        Node n2 = MakeNodeAt(-1, 0);
        Node n3 = MakeNodeAt(0, 1);

        // but we don't if we know it is deliberate edge concatenation
        // e.g. if the node is shared
        Assert.IsNull(GraphUtil.EdgeIntersect(n1, n2, n2, n3));
        Assert.IsNull(GraphUtil.EdgeIntersect(n2, n1, n2, n3));
        Assert.IsNull(GraphUtil.EdgeIntersect(n1, n2, n3, n2));
        Assert.IsNull(GraphUtil.EdgeIntersect(n2, n1, n3, n2));
    }

    [Test]
    public void TestEdgeIntersect_Edges()
    {
        {
            Node n1a = MakeNodeAt(1, 0);
            Node n2a = MakeNodeAt(-1, 0);
            Node n3a = MakeNodeAt(0, 1);

            // check same adjoining-edge behaviour as we checked with nodes
            Assert.IsNull(GraphUtil.EdgeIntersect(MakeEdge(n1a, n2a), MakeEdge(n2a, n3a)));
            Assert.IsNull(GraphUtil.EdgeIntersect(MakeEdge(n2a, n1a), MakeEdge(n2a, n3a)));
            Assert.IsNull(GraphUtil.EdgeIntersect(MakeEdge(n1a, n2a), MakeEdge(n3a, n2a)));
            Assert.IsNull(GraphUtil.EdgeIntersect(MakeEdge(n2a, n1a), MakeEdge(n3a, n2a)));
        }

        // just repeat a couple of the above tests and check we get the same t values
        float[] values = { 1e-6f, 2e-6f, 5e-6f, 1e-5f, 2e-5f, 5e-5f, 1e-4f, 2e-4f, 5e-4f, 1e-3f, 2e-3f, 5e-3f, 1e-2f, 2e-2f, 5e-2f, 1e-1f, 2e-1f, 5e-1f };

        Node n1 = MakeNodeAt(0, 0);
        Node n2 = MakeNodeAt(1, 0);
        DirectedEdge e1 = MakeEdge(n1, n2);

        foreach (float f in values)
        {
            Node n3 = MakeNodeAt(f, 0.5f);
            Node n4 = MakeNodeAt(f, -0.5f);
            DirectedEdge e2 = MakeEdge(n3, n4);

            {
                IntersectionResult ret = GraphUtil.EdgeIntersect(e1, e2);

                Assert.AreEqual(e1, ret.Edge1);
                Assert.AreEqual(e2, ret.Edge2);
                Assert.AreEqual(f, ret.T1, 1e-8);
                Assert.AreEqual(0.5f, ret.T2, 1e-8);
            }

            {
                IntersectionResult ret = GraphUtil.EdgeIntersect(e2, e1);

                Assert.AreEqual(e1, ret.Edge2);
                Assert.AreEqual(e2, ret.Edge1);
                Assert.AreEqual(0.5f, ret.T1, 1e-8);
                Assert.AreEqual(f, ret.T2, 1e-8);
            }
        }
    }

    // ------------------

    Node MakeNodeAt(float x, float y)
    {
        return MakeRadiusNodeAt(x, y, 0.0f);
    }

    Node MakeRadiusNodeAt(float x, float y, float radius)
    {
        Node ret = new Node("", "", radius)
        {
            Position = new Vector2(x, y)
        };

        return ret;
    }

    private DirectedEdge MakeEdge(Node n1, Node n2)
    {
        return new DirectedEdge(n1, n2, 0, 0, 0);
    }
}
using Assets.Generation.G;
using Assets.Generation.Gen;
using Assets.Generation.Stepping;
using Assets.Generation.U;
using NUnit.Framework;
using UnityEngine;
using static Assets.Generation.U.Util;

public class RelaxerStepper_CGTest
{
    private GeneratorConfig m_config;

    [SetUp]
    public void SetUp()
    {
        m_config = new GeneratorConfig();
        // run it to a tighter convergence than usual
        m_config.RelaxationMoveTarget = 1e-3f;
        m_config.RelaxationForceTarget = 1e-4f;
        m_config.RelaxationMinimumSeparation = 0;
    }

    [Test]
    public void TestSingleEdgeRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("n1", "", "", 0);
        INode n2 = g.AddNode("n2", "", "", 0);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);

        // a possible triangle and two single-connected nodes
        DirectedEdge e12 = g.Connect(n1, n2, 100, 100, 0);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;

        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        while (ret.Status == StepperController.Status.Iterate) ;

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // with a possible triangle and no other constraints
        // should get all lengths within a small tolerance of the target

        // however how close we actually get is phenomonological (e.g. just what I see while writing this)
        // but we can return with higher expectations later if required

        Assert.AreEqual(100, e12.Length(), 1);
    }

    [Test]
    public void TestEdgeRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("n1", "", "", 0);
        INode n2 = g.AddNode("n2", "", "", 0);
        INode n3 = g.AddNode("n3", "", "", 0);
        INode n4 = g.AddNode("n4", "", "", 0);
        INode n5 = g.AddNode("n5", "", "", 0);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);
        n3.Position = new Vector2(0, -100);
        n4.Position = new Vector2(100, 0);
        n5.Position = new Vector2(0, 100);

        // a possible triangle and two single-connected nodes
        DirectedEdge e12 = g.Connect(n1, n2, 100, 100, 0);
        DirectedEdge e23 = g.Connect(n2, n3, 80, 80, 0);
        DirectedEdge e31 = g.Connect(n3, n1, 60, 60, 0);
        DirectedEdge e34 = g.Connect(n3, n4, 120, 120, 0);
        DirectedEdge e15 = g.Connect(n1, n5, 40, 40, 0);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;

        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // with a possible triangle and no other constraints
        // should get all lengths within a small tolerance of the target

        // however how close we actually get is phenomonological (e.g. just what I see while writing this)
        // but we can return with higher expectations later if required

        Assert.AreEqual(100, e12.Length(), 1);
        Assert.AreEqual(80, e23.Length(), 1);
        Assert.AreEqual(60, e31.Length(), 1);
        Assert.AreEqual(120, e34.Length(), 1);
        Assert.AreEqual(40, e15.Length(), 1);
    }

    [Test]
    public void TestEdgeContradictionRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("n1", "", "", 0);
        INode n2 = g.AddNode("n2", "", "", 0);
        INode n3 = g.AddNode("n3", "", "", 0);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);
        n3.Position = new Vector2(0, -100);

        // an impossible triangle
        DirectedEdge e12 = g.Connect(n1, n2, 100, 100, 0);
        DirectedEdge e23 = g.Connect(n2, n3, 40, 40, 0);
        DirectedEdge e31 = g.Connect(n3, n1, 40, 40, 0);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should arrive at a compromise, close to linear

        Util.NEDRet nr = Util.NodeEdgeDistDetailed(n3.Position, e12.Start.Position, e12.End.Position, true);
        Assert.IsTrue(nr.Dist < 0.0001f);
    }

    [Test]
    public void TestNodeWideSeparationRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("n1", "", "", 10.0f);
        INode n2 = g.AddNode("n2", "", "", 10.0f);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should take a single cycle to see that nothing needs to move
        Assert.AreEqual(0, n1.Position.x, 0);
        Assert.AreEqual(0, n1.Position.y, 0);
        Assert.AreEqual(-100, n2.Position.x, 0);
        Assert.AreEqual(0, n2.Position.y, 0);
    }

    [Test]
    public void TestNodeTooCloseRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("n1", "", "", 10.0f);
        INode n2 = g.AddNode("n2", "", "", 10.0f);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-1, 0);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        float dist = (n2.Position - n1.Position).magnitude;

        Assert.AreEqual(20.0f, dist, 0.1f);
    }

    [Test]
    public void TestEdgeWideSeparationRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("edge1start", "", "", 10.0f);
        INode n2 = g.AddNode("edge1end", "", "", 10.0f);
        INode n3 = g.AddNode("edge2start", "", "", 10.0f);
        INode n4 = g.AddNode("edge2end", "", "", 10.0f);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(0, 20);
        n3.Position = new Vector2(100, 0);
        n4.Position = new Vector2(100, 20);

        g.Connect(n1, n2, 20, 20, 10);
        g.Connect(n3, n4, 20, 20, 10);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should take a single cycle to see that nothing needs to move
        Assert.AreEqual(0, n1.Position.x, 0);
        Assert.AreEqual(0, n1.Position.y, 0);
        Assert.AreEqual(0, n2.Position.x, 0);
        Assert.AreEqual(20, n2.Position.y, 0);
        Assert.AreEqual(100, n3.Position.x, 0);
        Assert.AreEqual(0, n3.Position.y, 0);
        Assert.AreEqual(100, n4.Position.x, 0);
        Assert.AreEqual(20, n4.Position.y, 0);
    }

    [Test]
    public void TestEdgeTooCloseRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("edge1start", "", "", 10.0f);
        INode n2 = g.AddNode("edge1end", "", "", 10.0f);
        INode n3 = g.AddNode("edge2start", "", "", 10.0f);
        INode n4 = g.AddNode("edge2end", "", "", 10.0f);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(0, 20);
        n3.Position = new Vector2(1, 0);
        n4.Position = new Vector2(1, 20);

        g.Connect(n1, n2, 20, 20, 10);
        g.Connect(n3, n4, 20, 20, 10);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should have just slid sideways along X
        Assert.AreEqual(20, n3.Position.x - n1.Position.x, 0.1);
        Assert.AreEqual(20, n4.Position.x - n2.Position.x, 0.1);

        Assert.AreEqual(0, n1.Position.y, 1e-3);
        Assert.AreEqual(20, n2.Position.y, 1e-3);
        Assert.AreEqual(0, n3.Position.y, 1e-3);
        Assert.AreEqual(20, n4.Position.y, 1e-3);
    }

    [Test]
    public void TestEdgeNodeTooCloseRelaxation()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("edge1start", "", "", 0.0f);
        INode n2 = g.AddNode("edge1end", "", "", 0.0f);
        INode n3 = g.AddNode("node", "", "", 10.0f);

        // edge long enough that there is no n1->n3 or n2->n3 interaction
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(0, 100);
        n3.Position = new Vector2(1, 50);

        g.Connect(n1, n2, 100, 100, 10);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should have just slid sideways along X
        Assert.AreEqual(20, n3.Position.x - n1.Position.x, 0.1);

        Assert.AreEqual(0, n1.Position.y, 0);
        Assert.AreEqual(100, n2.Position.y, 0);
        Assert.AreEqual(50, n3.Position.y, 0);
    }

    [Test]
    public void TestCrossingEdge_Error()
    {
        Graph g = new Graph(null);

        INode n1 = g.AddNode("edge1start", "", "", 10.0f);
        INode n2 = g.AddNode("edge1end", "", "", 10.0f);
        INode n3 = g.AddNode("edge2start", "", "", 10.0f);
        INode n4 = g.AddNode("edge2end", "", "", 10.0f);

        // two clearly crossing edges
        n1.Position = new Vector2(0, -100);
        n2.Position = new Vector2(0, 100);
        n3.Position = new Vector2(-100, 0);
        n4.Position = new Vector2(100, 0);

        g.Connect(n1, n2, 100, 100, 10);
        g.Connect(n3, n4, 100, 100, 10);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        Assert.AreEqual(StepperController.Status.StepOutFailure, ret.Status);

        // should detect immediately
        Assert.IsTrue(ret.Log.Contains("crossing edges"));
    }

    [Test]
    public void TestDegeneracy()
    {
        // edge lengths of zero and edge-node distances of zero shouldn't crash anything and should
        // even relax as long as there is some other force to pull them apart

        // zero length edge
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("edgesstart", "", "", 10.0f);
            INode n2 = g.AddNode("edgesmiddle", "", "", 10.0f);
            INode n3 = g.AddNode("edgesend", "", "", 10.0f);

            // zero length edge and a non-zero one attached at one end that will separate
            // the overlying nodes
            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(0, 0);
            n3.Position = new Vector2(-110, 0);

            g.Connect(n1, n2, 100, 100, 10);
            g.Connect(n2, n3, 100, 100, 10);

            RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
            rs.MaxIterationsPerStep = 1000;

            StepperController.StatusReportInner ret;
            // engine.RelaxerStepper_CG doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);

            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            Assert.AreEqual(100, (n1.Position - n2.Position).magnitude, 1);
            Assert.AreEqual(100, (n2.Position - n3.Position).magnitude, 1);
            Assert.IsTrue((n1.Position - n3.Position).magnitude > 20);
        }

        // zero node separation
        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("edgestart", "", "", 10.0f);
            INode n2 = g.AddNode("edgeend", "", "", 10.0f);
            INode n3 = g.AddNode("node", "", "", 10.0f);

            // two zero separation nodes and an edge attached to one that will separate
            // the overlying nodes
            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(110, 0);
            n3.Position = new Vector2(0, 0);

            g.Connect(n1, n2, 100, 100, 10);

            RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
            rs.MaxIterationsPerStep = 1000;

            StepperController.StatusReportInner ret;
            // engine.RelaxerStepper_CG doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);

            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            Assert.AreEqual(100, (n1.Position - n2.Position).magnitude, 1);
            Assert.IsTrue((n1.Position - n3.Position).magnitude > 20);
            Assert.IsTrue((n2.Position - n3.Position).magnitude > 20);
        }
    }

    [Test]
    public void TestAdjoiningEdgeOverridesRadii()
    {
        Graph g = new Graph(null);
        INode n1 = g.AddNode("n1", "", "", 100);
        INode n2 = g.AddNode("n2", "", "", 100);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);

        // edge wants distance of 100, node-radii want 200 but node-radii
        // should be ignored between connected nodes
        DirectedEdge e12 = g.Connect(n1, n2, 100, 100, 0);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        Assert.AreEqual(100, e12.Length(), 1);
    }

    [Test]
    public void TestNonAdjoiningEdgesOverrideRadii()
    {
        Graph g = new Graph(null);
        INode n1 = g.AddNode("n1", "", "", 6);
        INode n2 = g.AddNode("n2", "", "", 0);
        INode n3 = g.AddNode("n3", "", "", 0);
        INode n4 = g.AddNode("n4", "", "", 0);
        INode n5 = g.AddNode("n5", "", "", 0);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(10, 0);
        n3.Position = new Vector2(10, 10);
        n4.Position = new Vector2(20, 10);
        n5.Position = new Vector2(20, 20);

        // edges wants distances of 2, n1 radius wants 6 but shortest path through
        // graph should come out below that (for n2, n3) and let them get closer
        DirectedEdge e12 = g.Connect(n1, n2, 2, 2, 0);
        DirectedEdge e23 = g.Connect(n2, n3, 2, 2, 0);
        DirectedEdge e34 = g.Connect(n3, n4, 2, 2, 0);
        DirectedEdge e45 = g.Connect(n4, n5, 2, 2, 0);

        RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
        rs.MaxIterationsPerStep = 1000;

        StepperController.StatusReportInner ret;
        // engine.RelaxerStepper_CG doesn't use previous status
        ret = rs.Step(StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // all edges should be able to reach ~2 even if that violates the radius of n1
        Assert.AreEqual(2, e12.Length(), .1);
        Assert.AreEqual(2, e23.Length(), .1);
        Assert.AreEqual(2, e34.Length(), .1);
        Assert.AreEqual(2, e45.Length(), .1);

        // n4 and n5 hve enough edge length to get far enough from n1 and should do so
        Assert.IsTrue((n1.Position - n4.Position).magnitude >= 5.99);
        Assert.IsTrue((n1.Position - n5.Position).magnitude >= 5.99);
    }

    [Test]
    public void TestMinimumSeparation()
    {
        {
            Graph g = new Graph(null);
            INode n1 = g.AddNode("n1", "", "", 10.0f);
            INode n2 = g.AddNode("n2", "", "", 10.0f);

            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(-1, 0);

            // add 1 unit of extra separation
            m_config.RelaxationMinimumSeparation = 1;
            RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
            rs.MaxIterationsPerStep = 1000;

            StepperController.StatusReportInner ret;
            // engine.RelaxerStepper_CG doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);

            // simple case should succeed
            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            float dist = (n2.Position - n1.Position).magnitude;

            // should get rad+rad+separation
            Assert.AreEqual(21.0f, dist, 0.1f);
        }

        {
            Graph g = new Graph(null);

            INode n1 = g.AddNode("edge1start", "", "", 10.0f);
            INode n2 = g.AddNode("edge1end", "", "", 10.0f);
            INode n3 = g.AddNode("node", "", "", 10.0f);

            // edge long enough that there is no n1->n3 or n2->n3 interaction
            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(0, 100);
            n3.Position = new Vector2(1, 50);

            g.Connect(n1, n2, 100, 100, 10);

            // add an extra separation of 1 unit
            m_config.RelaxationMinimumSeparation = 1;
            RelaxerStepper_CG rs = new RelaxerStepper_CG(null, g, m_config);
            rs.MaxIterationsPerStep = 1000;

            StepperController.StatusReportInner ret;
            // engine.RelaxerStepper_CG doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);

            // simple case should succeed
            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            // should have just slid sideways along X
            // to a total dist of node radius + edge half-width +
            // separation
            Assert.AreEqual(21, n3.Position.x - n1.Position.x, 0.1);

            Assert.AreEqual(0, n1.Position.y, 0);
            Assert.AreEqual(100, n2.Position.y, 0);
            Assert.AreEqual(50, n3.Position.y, 0);
        }
    }
}
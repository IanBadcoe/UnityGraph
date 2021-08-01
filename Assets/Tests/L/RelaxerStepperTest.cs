using Assets.Generation.G;
using Assets.Generation.Gen;
using Assets.Generation.Stepping;
using NUnit.Framework;
using UnityEngine;

public class RelaxerStepperTest
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
    public void TestEdgeRelaxation()
    {
        Graph g = new Graph();

        INode n1 = g.AddNode("n1", "", "", 0, null);
        INode n2 = g.AddNode("n2", "", "", 0, null);
        INode n3 = g.AddNode("n3", "", "", 0, null);
        INode n4 = g.AddNode("n4", "", "", 0, null);
        INode n5 = g.AddNode("n5", "", "", 0, null);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);
        n3.Position = new Vector2(0, -100);
        n4.Position = new Vector2(100, 0);
        n5.Position = new Vector2(0, 100);

        // a possible triangle and two single-connected nodes
        DirectedEdge e12 = g.Connect(n1, n2, 100, 100, 0, null);
        DirectedEdge e23 = g.Connect(n2, n3, 80, 80, 0, null);
        DirectedEdge e31 = g.Connect(n3, n1, 60, 60, 0, null);
        DirectedEdge e34 = g.Connect(n3, n4, 120, 120, 0, null);
        DirectedEdge e15 = g.Connect(n1, n5, 40, 40, 0, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;

        int count = 0;
        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // with a possible triangle and no other constraints
        // should get all lengths within a small tolerance of the target

        // however how close we actually get is phenomonological (e.g. just what I see while writing this)
        // but we can return with higher expectations later if required

        // again just what I saw initially, these are broadly parabolic forces so a better optimisation algorithm
        // would do this _much_ faster, OTOH these forces have edges and the complexity of things moving around each
        // other may mean this is really a much more complex force landscape than you'd think
        Assert.IsTrue(count < 30000);

        Assert.AreEqual(100, e12.Length(), 1);
        Assert.AreEqual(80, e23.Length(), 1);
        Assert.AreEqual(60, e31.Length(), 1);
        Assert.AreEqual(120, e34.Length(), 1);
        Assert.AreEqual(40, e15.Length(), 1);
    }

    [Test]
    public void TestEdgeContradictionRelaxation()
    {
        Graph g = new Graph();

        INode n1 = g.AddNode("n1", "", "", 0, null);
        INode n2 = g.AddNode("n2", "", "", 0, null);
        INode n3 = g.AddNode("n3", "", "", 0, null);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);
        n3.Position = new Vector2(0, -100);

        // an impossible triangle
        DirectedEdge e12 = g.Connect(n1, n2, 100, 100, 0, null);
        DirectedEdge e23 = g.Connect(n2, n3, 40, 40, 0, null);
        DirectedEdge e31 = g.Connect(n3, n1, 40, 40, 0, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;
        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should arrive at a compromise, close to linear with
        // n1 -> n2 a bit compressed and the other two edges stretched

        // see comments in testEdgeRelaxation about how these accuracies and count aren't at all definitive

        Assert.IsTrue(count < 40000);

        Assert.AreEqual(90, e12.Length(), 2);
        Assert.AreEqual(45, e23.Length(), 1);
        Assert.AreEqual(45, e31.Length(), 1);
    }

    [Test]
    public void TestNodeWideSeparationRelaxation()
    {
        Graph g = new Graph();

        INode n1 = g.AddNode("n1", "", "", 10.0f, null);
        INode n2 = g.AddNode("n2", "", "", 10.0f, null);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;

        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should take a single cycle to see that nothing needs to move
        Assert.AreEqual(1, count);
        Assert.AreEqual(0, n1.Position.x, 0);
        Assert.AreEqual(0, n1.Position.y, 0);
        Assert.AreEqual(-100, n2.Position.x, 0);
        Assert.AreEqual(0, n2.Position.y, 0);
    }

    [Test]
    public void TestNodeTooCloseRelaxation()
    {
        Graph g = new Graph();

        INode n1 = g.AddNode("n1", "", "", 10.0f, null);
        INode n2 = g.AddNode("n2", "", "", 10.0f, null);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-1, 0);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;

        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // shouldn't take many cycles to bring them close to the target separation
        Assert.IsTrue(count < 100);

        float dist = (n2.Position - n1.Position).magnitude;

        Assert.AreEqual(20.0f, dist, 0.1f);
    }

    [Test]
    public void TestEdgeWideSeparationRelaxation()
    {
        Graph g = new Graph();

        INode n1 = g.AddNode("edge1start", "", "", 10.0f, null);
        INode n2 = g.AddNode("edge1end", "", "", 10.0f, null);
        INode n3 = g.AddNode("edge2start", "", "", 10.0f, null);
        INode n4 = g.AddNode("edge2end", "", "", 10.0f, null);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(0, 20);
        n3.Position = new Vector2(100, 0);
        n4.Position = new Vector2(100, 20);

        g.Connect(n1, n2, 20, 20, 10, null);
        g.Connect(n3, n4, 20, 20, 10, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;

        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // should take a single cycle to see that nothing needs to move
        Assert.AreEqual(1, count);
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
        Graph g = new Graph();

        INode n1 = g.AddNode("edge1start", "", "", 10.0f, null);
        INode n2 = g.AddNode("edge1end", "", "", 10.0f, null);
        INode n3 = g.AddNode("edge2start", "", "", 10.0f, null);
        INode n4 = g.AddNode("edge2end", "", "", 10.0f, null);

        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(0, 20);
        n3.Position = new Vector2(1, 0);
        n4.Position = new Vector2(1, 20);

        g.Connect(n1, n2, 20, 20, 10, null);
        g.Connect(n3, n4, 20, 20, 10, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;

        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // shouldn't take long to push the edges apart
        Assert.IsTrue(count < 50);
        // should have just slid sideways along X
        Assert.AreEqual(20, n3.Position.x - n1.Position.x, 0.1);
        Assert.AreEqual(20, n4.Position.x - n2.Position.x, 0.1);

        Assert.AreEqual(0, n1.Position.y, 0);
        Assert.AreEqual(20, n2.Position.y, 0);
        Assert.AreEqual(0, n3.Position.y, 0);
        Assert.AreEqual(20, n4.Position.y, 0);
    }

    [Test]
    public void TestEdgeNodeTooCloseRelaxation()
    {
        Graph g = new Graph();

        INode n1 = g.AddNode("edge1start", "", "", 0.0f, null);
        INode n2 = g.AddNode("edge1end", "", "", 0.0f, null);
        INode n3 = g.AddNode("node", "", "", 10.0f, null);

        // edge long enough that there is no n1->n3 or n2->n3 interaction
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(0, 100);
        n3.Position = new Vector2(1, 50);

        g.Connect(n1, n2, 100, 100, 10, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;

        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // takes a little time to push the edge and node apart
        Assert.IsTrue(count < 120);
        // should have just slid sideways along X
        Assert.AreEqual(20, n3.Position.x - n1.Position.x, 0.1);

        Assert.AreEqual(0, n1.Position.y, 0);
        Assert.AreEqual(100, n2.Position.y, 0);
        Assert.AreEqual(50, n3.Position.y, 0);
    }

    [Test]
    public void TestCrossingEdge_Error()
    {
        Graph g = new Graph();

        INode n1 = g.AddNode("edge1start", "", "", 10.0f, null);
        INode n2 = g.AddNode("edge1end", "", "", 10.0f, null);
        INode n3 = g.AddNode("edge2start", "", "", 10.0f, null);
        INode n4 = g.AddNode("edge2end", "", "", 10.0f, null);

        // two clearly crossing edges
        n1.Position = new Vector2(0, -100);
        n2.Position = new Vector2(0, 100);
        n3.Position = new Vector2(-100, 0);
        n4.Position = new Vector2(100, 0);

        g.Connect(n1, n2, 100, 100, 10, null);
        g.Connect(n3, n4, 100, 100, 10, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;

        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        Assert.AreEqual(StepperController.Status.StepOutFailure, ret.Status);

        // should detect immediately
        Assert.IsTrue(count == 1);
        Assert.IsTrue(ret.Log.Contains("crossing edges"));
    }

    [Test]
    public void TestDegeneracy()
    {
        // edge lengths of zero and edge-node distances of zero shouldn't crash anything and should
        // even relax as long as there is some other force to pull them apart

        // zero length edge
        {
            Graph g = new Graph();

            INode n1 = g.AddNode("edgesstart", "", "", 10.0f, null);
            INode n2 = g.AddNode("edgesmiddle", "", "", 10.0f, null);
            INode n3 = g.AddNode("edgesend", "", "", 10.0f, null);

            // zero length edge and a non-zero one attached at one end that will separate
            // the overlying nodes
            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(0, 0);
            n3.Position = new Vector2(-110, 0);

            g.Connect(n1, n2, 100, 100, 10, null);
            g.Connect(n2, n3, 100, 100, 10, null);

            RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

            StepperController.StatusReportInner ret;
            int count = 0;

            do
            {
                count++;
                // engine.RelaxerStepper doesn't use previous status
                ret = rs.Step(StepperController.Status.Iterate);
            }
            while (ret.Status == StepperController.Status.Iterate);

            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            // rather a lot?
            Assert.IsTrue(count < 50000);
            Assert.AreEqual(100, (n1.Position - n2.Position).magnitude, 1);
            Assert.AreEqual(100, (n2.Position - n3.Position).magnitude, 1);
            Assert.IsTrue((n1.Position - n3.Position).magnitude > 20);
        }

        // zero node separation
        {
            Graph g = new Graph();

            INode n1 = g.AddNode("edgestart", "", "", 10.0f, null);
            INode n2 = g.AddNode("edgeend", "", "", 10.0f, null);
            INode n3 = g.AddNode("node", "", "", 10.0f, null);

            // two zero separation nodes and an edge attached to one that will separate
            // the overlying nodes
            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(110, 0);
            n3.Position = new Vector2(0, 0);

            g.Connect(n1, n2, 100, 100, 10, null);

            RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

            StepperController.StatusReportInner ret;
            int count = 0;

            do
            {
                count++;
                // engine.RelaxerStepper doesn't use previous status
                ret = rs.Step(StepperController.Status.Iterate);
            }
            while (ret.Status == StepperController.Status.Iterate);

            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            // rather a lot?
            Assert.IsTrue(count < 10000);
            Assert.AreEqual(100, (n1.Position - n2.Position).magnitude, 1);
            Assert.IsTrue((n1.Position - n3.Position).magnitude > 20);
            Assert.IsTrue((n2.Position - n3.Position).magnitude > 20);
        }
    }

    [Test]
    public void TestAdjoiningEdgeOverridesRadii()
    {
        Graph g = new Graph();
        INode n1 = g.AddNode("n1", "", "", 100, null);
        INode n2 = g.AddNode("n2", "", "", 100, null);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(-100, 0);

        // edge wants distance of 100, node-radii want 200 but node-radii
        // should be ignored between connected nodes
        DirectedEdge e12 = g.Connect(n1, n2, 100, 100, 0, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;

        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        Assert.AreEqual(1, count);

        Assert.AreEqual(100, e12.Length(), 1);
    }

    [Test]
    public void TestNonAdjoiningEdgesOverrideRadii()
    {
        Graph g = new Graph();
        INode n1 = g.AddNode("n1", "", "", 6, null);
        INode n2 = g.AddNode("n2", "", "", 0, null);
        INode n3 = g.AddNode("n3", "", "", 0, null);
        INode n4 = g.AddNode("n4", "", "", 0, null);
        INode n5 = g.AddNode("n5", "", "", 0, null);

        // place them non-overlapping and separated in both dimensions
        n1.Position = new Vector2(0, 0);
        n2.Position = new Vector2(10, 0);
        n3.Position = new Vector2(10, 10);
        n4.Position = new Vector2(20, 10);
        n5.Position = new Vector2(20, 20);

        // edges wants distances of 2, n1 radius wants 6 but shortest path through
        // graph should come out below that (for n2, n3) and let them get closer
        DirectedEdge e12 = g.Connect(n1, n2, 2, 2, 0, null);
        DirectedEdge e23 = g.Connect(n2, n3, 2, 2, 0, null);
        DirectedEdge e34 = g.Connect(n3, n4, 2, 2, 0, null);
        DirectedEdge e45 = g.Connect(n4, n5, 2, 2, 0, null);

        RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

        StepperController.StatusReportInner ret;
        int count = 0;
        do
        {
            count++;
            // engine.RelaxerStepper doesn't use previous status
            ret = rs.Step(StepperController.Status.Iterate);
        }
        while (ret.Status == StepperController.Status.Iterate);

        Assert.IsTrue(count < 3000);

        // simple case should succeed
        Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

        // all edges should be able to reach ~2 even if that violates the radius of n1
        Assert.AreEqual(2, e12.Length(), .1);
        Assert.AreEqual(2, e23.Length(), .1);
        Assert.AreEqual(2, e34.Length(), .1);
        Assert.AreEqual(2, e45.Length(), .1);

        // n4 and n5 hve enough edge length to get far enough from n1 and should do so
        Assert.IsTrue((n1.Position - n4.Position).magnitude >= 6);
        Assert.IsTrue((n1.Position - n5.Position).magnitude >= 6);
    }

    [Test]
    public void TestMinimumSeparation()
    {
        {
            Graph g = new Graph();
            INode n1 = g.AddNode("n1", "", "", 10.0f, null);
            INode n2 = g.AddNode("n2", "", "", 10.0f, null);

            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(-1, 0);

            // add 1 unit of extra separation
            m_config.RelaxationMinimumSeparation = 1;
            RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

            StepperController.StatusReportInner ret;
            int count = 0;

            do
            {
                count++;
                // engine.RelaxerStepper doesn't use previous status
                ret = rs.Step(StepperController.Status.Iterate);
            }
            while (ret.Status == StepperController.Status.Iterate);

            // simple case should succeed
            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            // shouldn't take many cycle to bring them close to the target separation
            Assert.IsTrue(count < 100);

            float dist = (n2.Position - n1.Position).magnitude;

            // should get rad+rad+separation
            Assert.AreEqual(21.0f, dist, 0.1f);
        }

        {
            Graph g = new Graph();

            INode n1 = g.AddNode("edge1start", "", "", 10.0f, null);
            INode n2 = g.AddNode("edge1end", "", "", 10.0f, null);
            INode n3 = g.AddNode("node", "", "", 10.0f, null);

            // edge long enough that there is no n1->n3 or n2->n3 interaction
            n1.Position = new Vector2(0, 0);
            n2.Position = new Vector2(0, 100);
            n3.Position = new Vector2(1, 50);

            g.Connect(n1, n2, 100, 100, 10, null);

            // add an extra separation of 1 unit
            m_config.RelaxationMinimumSeparation = 1;
            RelaxerStepper rs = new RelaxerStepper(null, g, m_config);

            StepperController.StatusReportInner ret;
            int count = 0;
            do
            {
                count++;
                // engine.RelaxerStepper doesn't use previous status
                ret = rs.Step(StepperController.Status.Iterate);
            }
            while (ret.Status == StepperController.Status.Iterate);

            // simple case should succeed
            Assert.AreEqual(StepperController.Status.StepOutSuccess, ret.Status);

            // takes a little time to push the edge and node apart
            Assert.IsTrue(count < 130);
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
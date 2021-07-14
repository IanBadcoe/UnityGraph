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
using Assets.Generation;
using Assets.Generation.GeomRep;
using Assets.Extensions;

public class IntersectorTest
{
    private readonly Intersector m_intersector = new Intersector();
    
    class Fake : Curve
    {
        public readonly String Name;
        
        public Fake(String name)
            : base(0, 1)
        {
            Name = name;
        }


        protected override Vector2 ComputePos_Inner(float m_start_param)
        {
            return Vector2.zero;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object o)
        {
            // we're only going to want to compare unique modelling for the test cases
            return ReferenceEquals(this, o);
        }

        protected override float? FindParamForPoint_Inner(Vector2 first, float tol)
        {
            return null;
        }

        public override Curve CloneWithChangedParams(float start, float end)
        {
            return null;
        }

        public override Area BoundingArea()
        {
            return null;
        }

        public override Vector2 Tangent(float second)
        {
            return Vector2.zero;
        }

        public override Curve Merge(Curve c_after)
        {
            return null;
        }

        public override float Length()
        {
            return 0;
        }

        public override Vector2 ComputeNormal(float p)
        {
            return Vector2.zero;
        }
    }

    [Test]
    public void TestBuildAnnotationChains()
    {
        List<Curve> curves = new List<Curve>();

        Curve ca = new Fake("a");
        Curve cb = new Fake("b");
        Curve cc = new Fake("c");
        Curve cd = new Fake("d");
        Curve ce = new Fake("e");

        curves.Add(ca);
        curves.Add(cb);
        curves.Add(cc);
        curves.Add(cd);
        curves.Add(ce);

        Dictionary<Curve, Intersector.AnnotatedCurve> forward_annotations_map = new Dictionary<Curve, Intersector.AnnotatedCurve>();

        m_intersector.BuildAnnotationChains(curves, 1, forward_annotations_map);

        foreach (Curve c in curves)
        {
            Assert.IsNotNull(forward_annotations_map[c]);
            Assert.IsNotNull(forward_annotations_map[c]);
            Assert.AreEqual(1, forward_annotations_map[c].LoopNumber);
        }

        Assert.AreEqual(cb, forward_annotations_map[ca].Next.Curve);
        Assert.AreEqual(cc, forward_annotations_map[cb].Next.Curve);
        Assert.AreEqual(cd, forward_annotations_map[cc].Next.Curve);
        Assert.AreEqual(ce, forward_annotations_map[cd].Next.Curve);
        Assert.AreEqual(ca, forward_annotations_map[ce].Next.Curve);
    }

    [Test]
    public void TestSplitCurvesAtIntersections_TwoCirclesTwoPoints()
    {
        // circles meet at two points
        Curve cc1 = new CircleCurve(new Vector2(), 1);
        Curve cc2 = new CircleCurve(new Vector2(1, 0), 1);

        List<Curve> curves1 = new List<Curve>();
        curves1.Add(cc1);

        List<Curve> curves2 = new List<Curve>();
        curves2.Add(cc2);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

        // we cut each curve twice, technically we could anneal the original curve across its
        // join at 2PI -> 0.0 but we don't currently try anything clever like that
        Assert.AreEqual(3, curves1.Count);
        Assert.AreEqual(3, curves2.Count);

        Assert.IsTrue(curves1[0].EndPos().Equals(curves1[1].StartPos(), 1e-5f));
        Assert.IsTrue(curves1[1].EndPos().Equals(curves1[2].StartPos(), 1e-5f));
        Assert.IsTrue(curves1[2].EndPos().Equals(curves1[0].StartPos(), 1e-5f));
        Assert.IsTrue(curves2[0].EndPos().Equals(curves2[1].StartPos(), 1e-5f));
        Assert.IsTrue(curves2[1].EndPos().Equals(curves2[2].StartPos(), 1e-5f));
        Assert.IsTrue(curves2[2].EndPos().Equals(curves2[0].StartPos(), 1e-5f));

        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[0].EndParam, curves1[1].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[1].EndParam, curves1[2].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[2].EndParam, curves1[0].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[0].EndParam, curves2[1].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[1].EndParam, curves2[2].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[2].EndParam, curves2[0].StartParam, 1e-5f));
    }

    //[Test]
    //   public void testSplitCurvesAtIntersections_TwoCirclesOnePoint()
    //{
    //    // circles meet at one point
    //    Curve cc1 = new CircleCurve(new Vector2(), 1);
    //    Curve cc2 = new CircleCurve(new Vector2(2, 0), 1);

    //    List<Curve> curves1 = new List<>();
    //    curves1.add(cc1);

    //    List<Curve> curves2 = new List<>();
    //    curves2.add(cc2);

    //    m_intersector.splitCurvesAtIntersections(curves1, curves2, 1e-6);

    //    Assert.AreEqual(2, curves1.size());
    //    Assert.AreEqual(2, curves2.size());

    //    Assert.IsTrue(curves1.get(0).endPos().equals(curves1.get(1).startPos(), 1e-6));
    //    Assert.IsTrue(curves1.get(1).endPos().equals(curves1.get(0).startPos(), 1e-6));
    //    Assert.IsTrue(curves2.get(0).endPos().equals(curves2.get(1).startPos(), 1e-6));
    //    Assert.IsTrue(curves2.get(1).endPos().equals(curves2.get(0).startPos(), 1e-6));

    //    Assert.IsTrue(Util.clockAwareAngleCompare(curves1.get(0).EndParam, curves1.get(1).StartParam, 1e-6));
    //    Assert.IsTrue(Util.clockAwareAngleCompare(curves1.get(1).EndParam, curves1.get(0).StartParam, 1e-6));
    //    Assert.IsTrue(Util.clockAwareAngleCompare(curves2.get(0).EndParam, curves2.get(1).StartParam, 1e-6));
    //    Assert.IsTrue(Util.clockAwareAngleCompare(curves2.get(1).EndParam, curves2.get(0).StartParam, 1e-6));
    //}

    //[Test]
    //   public void testSplitCurvesAtIntersections_SameCircleTwice()
    //{
    //    // same circle twice
    //    Curve cc1 = new CircleCurve(new Vector2(), 1);
    //    Curve cc2 = new CircleCurve(new Vector2(), 1);

    //    List<Curve> curves1 = new List<>();
    //    curves1.add(cc1);

    //    List<Curve> curves2 = new List<>();
    //    curves2.add(cc2);

    //    m_intersector.splitCurvesAtIntersections(curves1, curves2, 1e-6);

    //    Assert.AreEqual(1, curves1.size());
    //    Assert.AreEqual(1, curves2.size());

    //    Assert.IsTrue(curves1.get(0).endPos().equals(curves1.get(0).startPos(), 1e-6));
    //    Assert.IsTrue(curves2.get(0).endPos().equals(curves2.get(0).startPos(), 1e-6));

    //    Assert.IsTrue(Util.clockAwareAngleCompare(curves1.get(0).EndParam, curves1.get(0).StartParam, 1e-6));
    //    Assert.IsTrue(Util.clockAwareAngleCompare(curves2.get(0).EndParam, curves2.get(0).StartParam, 1e-6));
    //}

    //[Test]
    //   public void testSplitCurvesAtIntersections_OneCircleHitsBreakInOther()
    //{
    //    // one circle hits existing break in other
    //    Curve cc1 = new CircleCurve(new Vector2(), 1);

    //    List<Curve> curves1 = new List<>();
    //    curves1.add(cc1);

    //    List<Curve> curves2 = new List<>();

    //    for (floata = 0; a < Math.PI * 2; a += 0.1)
    //    {
    //        Curve cc2 = new CircleCurve(new Vector2(Math.sin(a), Math.cos(a)), 1);

    //        curves2.add(cc2);
    //    }

    //    m_intersector.splitCurvesAtIntersections(curves1, curves2, 1e-6);

    //    for (int i = 0; i < curves1.size(); i++)
    //    {
    //        int next_i = (i + 1) % curves1.size();
    //        Assert.IsTrue(Util.clockAwareAngleCompare(curves1.get(i).EndParam, curves1.get(next_i).StartParam, 1e-6));
    //        Assert.IsTrue(curves1.get(i).endPos().equals(curves1.get(next_i).startPos(), 1e-6));
    //    }
    //}

    //[Test]
    //   public void testFindSplices()
    //{
    //    Curve cc1 = new CircleCurve(new Vector2(), 1);
    //    Curve cc2 = new CircleCurve(new Vector2(1, 0), 1);

    //    List<Curve> curves1 = new List<>();
    //    curves1.add(cc1);

    //    List<Curve> curves2 = new List<>();
    //    curves2.add(cc2);

    //    m_intersector.splitCurvesAtIntersections(curves1, curves2, 1e-6);

    //    HashMap<Curve, Intersector.AnnotatedCurve> forward_annotations_map = new HashMap<>();

    //    m_intersector.buildAnnotationChains(curves1, 1,
    //          forward_annotations_map);

    //    m_intersector.buildAnnotationChains(curves2, 2,
    //          forward_annotations_map);

    //    HashMap<Curve, Intersector.Splice> endSpliceMap = new HashMap<>();

    //    m_intersector.findSplices(curves1, curves2,
    //          forward_annotations_map,
    //          endSpliceMap,
    //          1e-6);

    //    // two splices, with two in and two out curves each
    //    Assert.AreEqual(4, endSpliceMap.size());

    //    HashSet<Intersector.Splice> unique = new HashSet<>();
    //    unique.addAll(endSpliceMap.values());

    //    Assert.AreEqual(2, unique.size());

    //    for (Intersector.Splice s : unique)
    //      {
    //    HashSet<Intersector.AnnotatedCurve> l1fset = new HashSet<>();
    //    HashSet<Intersector.AnnotatedCurve> l2fset = new HashSet<>();

    //    Intersector.AnnotatedCurve acl1f = s.Loop1Out;
    //    Intersector.AnnotatedCurve acl2f = s.Loop2Out;

    //    for (int i = 0; i < 4; i++)
    //    {
    //        l1fset.add(acl1f);
    //        l2fset.add(acl2f);

    //        acl1f = acl1f.Next;
    //        acl2f = acl2f.Next;
    //    }

    //    // although we stepped four times, the loops are of length 3 and we
    //    // shouldn't have found any more AnnotationCurves
    //    Assert.AreEqual(3, l1fset.size());
    //    Assert.AreEqual(3, l2fset.size());

    //    // loops of AnnotationCurves should be unique
    //    Assert.IsTrue(Collections.disjoint(l1fset, l2fset));

    //    HashSet<Curve> l1fcset = l1fset.stream().map(x->x.Curve).collect(Collectors.toCollection(HashSet::new));
    //    HashSet<Curve> l2fcset = l2fset.stream().map(x->x.Curve).collect(Collectors.toCollection(HashSet::new));

    //    // and l1 and l2 don't contain any of the same curves
    //    Assert.IsTrue(Collections.disjoint(l1fcset, l2fcset));
    //}
    //   }

    //   [Test]
    //   public void testTryFindIntersections()
    //{
    //    // one circle, expect 1, 0
    //    {
    //        CircleCurve cc = new CircleCurve(new Vector2(), 5);

    //        HashSet<Curve> all_curves = new HashSet<>();
    //        all_curves.add(cc);

    //        HashSet<Vector2> curve_joints = new HashSet<>();
    //        curve_joints.add(cc.startPos());

    //        List<OrderedPair<Curve, Integer>> ret =
    //              m_intersector.tryFindIntersections(
    //                    new Vector2(0, -5),
    //                    all_curves,
    //                    curve_joints,
    //                    10, 1e-6,
    //                    new Random(1)
    //              );

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(2, ret.size());
    //        Assert.AreEqual(cc, ret.get(0).First);
    //        Assert.AreEqual(1, (int)ret.get(0).Second);
    //        Assert.AreEqual(cc, ret.get(1).First);
    //        Assert.AreEqual(0, (int)ret.get(1).Second);
    //    }

    //    // two concentric circles, expect 1, 2, 1, 0
    //    {
    //        CircleCurve cc1 = new CircleCurve(new Vector2(), 5);
    //        CircleCurve cc2 = new CircleCurve(new Vector2(), 3);

    //        HashSet<Curve> all_curves = new HashSet<>();
    //        all_curves.add(cc1);
    //        all_curves.add(cc2);

    //        HashSet<Vector2> curve_joints = new HashSet<>();
    //        curve_joints.add(cc1.startPos());
    //        curve_joints.add(cc2.startPos());

    //        List<OrderedPair<Curve, Integer>> ret =
    //              m_intersector.tryFindIntersections(
    //                    new Vector2(0, 0),  // use centre to force hitting both circles
    //                    all_curves,
    //                    curve_joints,
    //                    10, 1e-6,
    //                    new Random(1)
    //              );

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(4, ret.size());
    //        Assert.AreEqual(cc1, ret.get(0).First);
    //        Assert.AreEqual(1, (int)ret.get(0).Second);
    //        Assert.AreEqual(cc2, ret.get(1).First);
    //        Assert.AreEqual(2, (int)ret.get(1).Second);
    //        Assert.AreEqual(cc2, ret.get(2).First);
    //        Assert.AreEqual(1, (int)ret.get(2).Second);
    //        Assert.AreEqual(cc1, ret.get(3).First);
    //        Assert.AreEqual(0, (int)ret.get(3).Second);
    //    }

    //    // two concentric circles, inner one -ve, expect 1, 0, 1, 0
    //    {
    //        CircleCurve cc1 = new CircleCurve(new Vector2(), 5);
    //        CircleCurve cc2 = new CircleCurve(new Vector2(), 3, CircleCurve.RotationDirection.Reverse);

    //        HashSet<Curve> all_curves = new HashSet<>();
    //        all_curves.add(cc1);
    //        all_curves.add(cc2);

    //        HashSet<Vector2> curve_joints = new HashSet<>();
    //        curve_joints.add(cc1.startPos());
    //        curve_joints.add(cc2.startPos());

    //        List<OrderedPair<Curve, Integer>> ret =
    //              m_intersector.tryFindIntersections(
    //                    new Vector2(0, 0),  // use centre to force hitting both circles
    //                    all_curves,
    //                    curve_joints,
    //                    10, 1e-6,
    //                    new Random(1)
    //              );

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(4, ret.size());
    //        Assert.AreEqual(cc1, ret.get(0).First);
    //        Assert.AreEqual(1, (int)ret.get(0).Second);
    //        Assert.AreEqual(cc2, ret.get(1).First);
    //        Assert.AreEqual(0, (int)ret.get(1).Second);
    //        Assert.AreEqual(cc2, ret.get(2).First);
    //        Assert.AreEqual(1, (int)ret.get(2).Second);
    //        Assert.AreEqual(cc1, ret.get(3).First);
    //        Assert.AreEqual(0, (int)ret.get(3).Second);
    //    }
    //}

    //[Test]
    //   public void testTryFindCurveIntersections()
    //{
    //    // this was in the above, no idea why I went to the lower level routine in there
    //    // but I need to do that now anyway...

    //    // one circle, built from two half-circles, should still work
    //    // expect 1, 0
    //    {
    //        CircleCurve cc1 = new CircleCurve(new Vector2(), 5, 0, Math.PI);
    //        CircleCurve cc2 = new CircleCurve(new Vector2(), 5, Math.PI, 2 * Math.PI);

    //        HashSet<Curve> all_curves = new HashSet<>();
    //        all_curves.add(cc1);
    //        all_curves.add(cc2);

    //        LineCurve lc = new LineCurve(new Vector2(-10, 0), new Vector2(1, 0), 20);

    //        List<OrderedPair<Curve, Integer>> ret =
    //              m_intersector.tryFindCurveIntersections(
    //                    lc,
    //                    all_curves);

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(2, ret.size());
    //        Assert.AreEqual(cc2, ret.get(0).First);
    //        Assert.AreEqual(1, (int)ret.get(0).Second);
    //        Assert.AreEqual(cc1, ret.get(1).First);
    //        Assert.AreEqual(0, (int)ret.get(1).Second);
    //    }

    //    // miss the circle, expect null
    //    {
    //        CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

    //        HashSet<Curve> all_curves = new HashSet<>();
    //        all_curves.add(cc1);

    //        LineCurve lc = new LineCurve(new Vector2(-10, 0), new Vector2(0, 1), 20);

    //        List<OrderedPair<Curve, Integer>> ret =
    //              m_intersector.tryFindCurveIntersections(
    //                    lc,
    //                    all_curves);

    //        assertNull(ret);
    //    }

    //    // clip the circle, to simplify the analysis we disregard these, expect null
    //    {
    //        CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

    //        HashSet<Curve> all_curves = new HashSet<>();
    //        all_curves.add(cc1);

    //        LineCurve lc = new LineCurve(new Vector2(-5, -5), new Vector2(0, 1), 20);

    //        List<OrderedPair<Curve, Integer>> ret =
    //              m_intersector.tryFindCurveIntersections(
    //                    lc,
    //                    all_curves);

    //        assertNull(ret);
    //    }
    //}

    //private static void checkLoop(Loop l, @SuppressWarnings("SameParameterValue") int exp_size)
    //   {
    //    Assert.AreEqual(exp_size, l.numCurves());

    //    Vector2 prev_end = l.getCurves().get(l.numCurves() - 1).endPos();

    //    for (Curve c : l.getCurves())
    //    {
    //        Assert.IsTrue(prev_end.equals(c.startPos(), 1e-6));
    //        prev_end = c.endPos();

    //        Assert.IsTrue(c instanceof CircleCurve);
    //    }
    //}

    //[Test]
    //   public void testUnion() throws Exception
    //{
    //      // nothing union nothing should equal nothing
    //      {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        assertNull(ret);
    //    }

    //      // something union nothing should equal something
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(1, ret.size());
    //        Assert.AreEqual(ls1, ret);
    //    }

    //      // nothing union something should equal something
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls2.add(l2);

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(1, ret.size());
    //        Assert.AreEqual(ls2, ret);
    //    }

    //      // union of two identical things should equal either one of them
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls2.add(l2);

    //        // paranoia
    //        Assert.AreEqual(ls1, ls2);

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(1, ret.size());
    //        Assert.AreEqual(ls1, ret);
    //    }

    //      // union of two overlapping circles should be one two-part curve
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(1, 0), 1));
    //        ls2.add(l2);

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(1, ret.size());
    //        Assert.AreEqual(2, ret.get(0).numCurves());
    //        Curve c1 = ret.get(0).getCurves().get(0);
    //        Curve c2 = ret.get(0).getCurves().get(1);
    //        Assert.IsTrue(c1 instanceof CircleCurve);
    //        Assert.IsTrue(c2 instanceof CircleCurve);

    //        CircleCurve cc1 = (CircleCurve)c1;
    //        CircleCurve cc2 = (CircleCurve)c2;

    //        // same radii
    //        Assert.AreEqual(1, cc1.Radius, 1e-6);
    //        Assert.AreEqual(1, cc2.Radius, 1e-6);

    //        // same direction
    //        Assert.AreEqual(CircleCurve.RotationDirection.Forwards, cc1.Rotation);
    //        Assert.AreEqual(CircleCurve.RotationDirection.Forwards, cc2.Rotation);

    //        // joined end-to-end
    //        Assert.IsTrue(cc1.startPos().equals(cc2.endPos(), 1e-6));
    //        Assert.IsTrue(cc2.startPos().equals(cc1.endPos(), 1e-6));

    //        CircleCurve left = cc1.Position.X < cc2.Position.X ? cc1 : cc2;
    //        CircleCurve right = cc1.Position.X > cc2.Position.X ? cc1 : cc2;

    //        Assert.AreEqual(new Vector2(0, 0), left.Position);
    //        Assert.AreEqual(new Vector2(1, 0), right.Position);

    //        Assert.AreEqual(Math.PI * 2 * 5 / 12, left.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 13 / 12, left.EndParam, 1e-6);

    //        Assert.AreEqual(Math.PI * 2 * 11 / 12, right.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 19 / 12, right.EndParam, 1e-6);
    //    }

    //      // union of two overlapping circles with holes in
    //      // should be one two-part curve around outside and two two-part curves in the interior
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1a = new Loop(new CircleCurve(new Vector2(), 1));
    //        Loop l1b = new Loop(new CircleCurve(new Vector2(), 0.3, CircleCurve.RotationDirection.Reverse));
    //        ls1.add(l1a);
    //        ls1.add(l1b);

    //        Loop l2a = new Loop(new CircleCurve(new Vector2(1, 0), 1));
    //        Loop l2b = new Loop(new CircleCurve(new Vector2(1, 0), 0.3, CircleCurve.RotationDirection.Reverse));
    //        ls2.add(l2a);
    //        ls2.add(l2b);

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(3, ret.size());

    //        checkLoop(ret.get(0), 2);
    //        checkLoop(ret.get(1), 2);
    //        checkLoop(ret.get(2), 2);
    //    }

    //      // osculating circles, outside each other
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(2, 0), 1));
    //        ls2.add(l2);

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(1, ret.size());
    //        Assert.AreEqual(2, ret.get(0).numCurves());
    //        Curve c1 = ret.get(0).getCurves().get(0);
    //        Curve c2 = ret.get(0).getCurves().get(1);
    //        Assert.IsTrue(c1 instanceof CircleCurve);
    //        Assert.IsTrue(c2 instanceof CircleCurve);

    //        CircleCurve cc1 = (CircleCurve)c1;
    //        CircleCurve cc2 = (CircleCurve)c2;

    //        // same radii
    //        Assert.AreEqual(1, cc1.Radius, 1e-6);
    //        Assert.AreEqual(1, cc2.Radius, 1e-6);

    //        // same direction
    //        Assert.AreEqual(CircleCurve.RotationDirection.Forwards, cc1.Rotation);
    //        Assert.AreEqual(CircleCurve.RotationDirection.Forwards, cc2.Rotation);

    //        // joined end-to-end
    //        Assert.IsTrue(cc1.startPos().equals(cc2.endPos(), 1e-6));
    //        Assert.IsTrue(cc2.startPos().equals(cc1.endPos(), 1e-6));

    //        CircleCurve left = cc1.Position.X < cc2.Position.X ? cc1 : cc2;
    //        CircleCurve right = cc1.Position.X > cc2.Position.X ? cc1 : cc2;

    //        Assert.AreEqual(new Vector2(0, 0), left.Position);
    //        Assert.AreEqual(new Vector2(2, 0), right.Position);

    //        Assert.AreEqual(Math.PI * 2 * 3 / 12, left.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 15 / 12, left.EndParam, 1e-6);

    //        Assert.AreEqual(Math.PI * 2 * 9 / 12, right.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 21 / 12, right.EndParam, 1e-6);
    //    }

    //      // osculating circles, outside each other
    //      // other way around
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(2, 0), 1));
    //        ls2.add(l2);

    //        LoopSet ret = m_intersector.union(ls2, ls1, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(1, ret.size());
    //        Assert.AreEqual(2, ret.get(0).numCurves());
    //        Curve c1 = ret.get(0).getCurves().get(0);
    //        Curve c2 = ret.get(0).getCurves().get(1);
    //        Assert.IsTrue(c1 instanceof CircleCurve);
    //        Assert.IsTrue(c2 instanceof CircleCurve);

    //        CircleCurve cc1 = (CircleCurve)c1;
    //        CircleCurve cc2 = (CircleCurve)c2;

    //        // same radii
    //        Assert.AreEqual(1, cc1.Radius, 1e-6);
    //        Assert.AreEqual(1, cc2.Radius, 1e-6);

    //        // same direction
    //        Assert.AreEqual(CircleCurve.RotationDirection.Forwards, cc1.Rotation);
    //        Assert.AreEqual(CircleCurve.RotationDirection.Forwards, cc2.Rotation);

    //        // joined end-to-end
    //        Assert.IsTrue(cc1.startPos().equals(cc2.endPos(), 1e-6));
    //        Assert.IsTrue(cc2.startPos().equals(cc1.endPos(), 1e-6));

    //        CircleCurve left = cc1.Position.X < cc2.Position.X ? cc1 : cc2;
    //        CircleCurve right = cc1.Position.X > cc2.Position.X ? cc1 : cc2;

    //        Assert.AreEqual(new Vector2(0, 0), left.Position);
    //        Assert.AreEqual(new Vector2(2, 0), right.Position);

    //        Assert.AreEqual(Math.PI * 2 * 3 / 12, left.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 15 / 12, left.EndParam, 1e-6);

    //        Assert.AreEqual(Math.PI * 2 * 9 / 12, right.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 21 / 12, right.EndParam, 1e-6);
    //    }

    //      // osculating circles, one smaller, reversed and inside the other
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(0.5, 0), 0.5, CircleCurve.RotationDirection.Reverse));
    //        ls2.add(l2);

    //        LoopSet ret = m_intersector.union(ls1, ls2, 1e-6, new Random(1));

    //        Assert.IsNotNull(ret);
    //        Assert.AreEqual(1, ret.size());
    //        Assert.AreEqual(2, ret.get(0).numCurves());
    //        Curve c1 = ret.get(0).getCurves().get(0);
    //        Curve c2 = ret.get(0).getCurves().get(1);
    //        Assert.IsTrue(c1 instanceof CircleCurve);
    //        Assert.IsTrue(c2 instanceof CircleCurve);

    //        CircleCurve cc1 = (CircleCurve)c1;
    //        CircleCurve cc2 = (CircleCurve)c2;

    //        // joined end-to-end
    //        Assert.IsTrue(cc1.startPos().equals(cc2.endPos(), 1e-6));
    //        Assert.IsTrue(cc2.startPos().equals(cc1.endPos(), 1e-6));

    //        CircleCurve left = cc1.Position.X < cc2.Position.X ? cc1 : cc2;
    //        CircleCurve right = cc1.Position.X > cc2.Position.X ? cc1 : cc2;

    //        // same radii
    //        Assert.AreEqual(1, left.Radius, 1e-6);
    //        Assert.AreEqual(0.5, right.Radius, 1e-6);

    //        // same direction
    //        Assert.AreEqual(CircleCurve.RotationDirection.Forwards, left.Rotation);
    //        Assert.AreEqual(CircleCurve.RotationDirection.Reverse, right.Rotation);

    //        Assert.AreEqual(new Vector2(0, 0), left.Position);
    //        Assert.AreEqual(new Vector2(0.5, 0), right.Position);

    //        Assert.AreEqual(Math.PI * 2 * 3 / 12, left.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 15 / 12, left.EndParam, 1e-6);

    //        Assert.AreEqual(Math.PI * 2 * 9 / 12, right.StartParam, 1e-6);
    //        Assert.AreEqual(Math.PI * 2 * 21 / 12, right.EndParam, 1e-6);
    //    }
    //}

    //[Test]
    //   public void testAnnotatedCurve()
    //{
    //    Curve c1 = new CircleCurve(new Vector2(), 1);
    //    Curve c2 = new CircleCurve(new Vector2(), 1);

    //    Intersector.AnnotatedCurve ac1 = new Intersector.AnnotatedCurve(c1, 1);
    //    Intersector.AnnotatedCurve ac1b = new Intersector.AnnotatedCurve(c1, 1);
    //    Intersector.AnnotatedCurve ac2 = new Intersector.AnnotatedCurve(c2, 1);

    //    Assert.AreEqual(c1.hashCode(), ac1.hashCode());

    //    Assert.IsTrue(ac1.equals(ac1b));
    //    assertFalse(ac1.equals(ac2));
    //    //noinspection EqualsBetweenInconvertibleTypes
    //    assertFalse(ac1.equals(0));
    //}

    //class IntersectorDummy1 extends Intersector
    //{
    //    @Override
    //      protected boolean extractInternalCurves(floattol, Random random,
    //                                              HashMap<Curve, AnnotatedCurve> forward_annotations_map, HashSet<Curve> all_curves,
    //                                              HashSet<AnnotatedCurve> open, HashSet<Vector2> curve_joints, float diameter)
    //{
    //    return false;
    //}
    //   }

    //   class IntersectorDummy2 extends Intersector
    //{
    //    @Override
    //      List<OrderedPair<Curve, Integer>>
    //      tryFindIntersections(
    //            Vector2 mid_point,
    //            HashSet<Curve> all_curves,
    //            HashSet<Vector2> curve_joints,
    //            floatdiameter, floattol,
    //            Random random)
    //      {
    //        return null;
    //    }
    //}

    //class IntersectorDummy3 extends Intersector
    //{
    //    @Override
    //      public boolean lineClearsPoints(LineCurve lc, HashSet<Vector2> curve_joints, floattol)
    //{
    //    return false;
    //}
    //   }

    //   [Test]
    //   public void testUnion_Errors()
    //{
    //    // if extractInternalCurves fails, we bail...
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Intersector i = new IntersectorDummy1();

    //        LoopSet ret = i.union(ls1, ls2, 1e-6, new Random(1));

    //        assertNull(ret);
    //    }

    //    // if tryFindIntersections fails, we bail
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(0.5, 0), 1));
    //        ls2.add(l2);

    //        Intersector i = new IntersectorDummy2();

    //        LoopSet ret = i.union(ls1, ls2, 1e-6, new Random(1));

    //        assertNull(ret);
    //    }

    //    // if tryFindIntersections fails, we bail
    //    {
    //        LoopSet ls1 = new LoopSet();
    //        LoopSet ls2 = new LoopSet();

    //        Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
    //        ls1.add(l1);

    //        Loop l2 = new Loop(new CircleCurve(new Vector2(0.5, 0), 1));
    //        ls2.add(l2);

    //        Intersector i = new IntersectorDummy3();

    //        LoopSet ret = i.union(ls1, ls2, 1e-6, new Random(1));

    //        assertNull(ret);
    //    }
    //}

    //[Test]
    //   public void testLineClearsPoints()
    //{
    //    LineCurve lc1 = new LineCurve(new Vector2(), new Vector2(1, 0), 10);
    //    LineCurve lc2 = new LineCurve(new Vector2(), new Vector2(1 / Math.sqrt(2), 1 / Math.sqrt(2)), 10);

    //    {
    //        HashSet<Vector2> hs = new HashSet<>();
    //        hs.add(new Vector2(1, 1));

    //        Assert.IsTrue(m_intersector.lineClearsPoints(lc1, hs, 1e-6));
    //        assertFalse(m_intersector.lineClearsPoints(lc2, hs, 1e-6));
    //    }

    //    {
    //        HashSet<Vector2> hs = new HashSet<>();
    //        hs.add(new Vector2(0, 0));

    //        assertFalse(m_intersector.lineClearsPoints(lc1, hs, 1e-6));
    //        assertFalse(m_intersector.lineClearsPoints(lc2, hs, 1e-6));
    //    }

    //    {
    //        HashSet<Vector2> hs = new HashSet<>();
    //        hs.add(new Vector2(2, 0));

    //        assertFalse(m_intersector.lineClearsPoints(lc1, hs, 1e-6));
    //        Assert.IsTrue(m_intersector.lineClearsPoints(lc2, hs, 1e-6));
    //    }
    //}

    //// This one asserts because it somehow tries to make a discontinuous loop
    //// but, pragmatically, I don't need this test yet
    ////   [Test]
    ////   public void testUnion_ManyPoints()
    ////   {
    ////      LoopSet ls1 = new LoopSet();
    ////
    ////      Random r = new Random(1);
    ////
    ////      for(int i = 0; i < 1000; i++)
    ////      {
    ////         LoopSet ls2 = new LoopSet();
    ////
    ////         Loop l2 = new Loop(new CircleCurve(new XY(r.nextfloat(), r.nextfloat()), .1));
    ////         ls2.add(l2);
    ////
    ////         // try to make sure we have some lines hit some points
    ////         // (to his that return false in lineClearsPoints)
    ////         ls1 = m_intersector.union(ls1, ls2, 1e-2, r);
    ////
    ////         Assert.IsNotNull(ls1);
    ////      }
    ////   }

    //// This reproduces a numerical precision problem I had when I was placing the rectangle for a corridor
    //// so that it's corners exactly hit the perimeter of the circle at the corridor junctions
    //// this meant (I believe) that intersection tests could detect one, both or neither of the rectangle edges
    //// adjoining the corner as hitting the circle, with hillarious consequences
    ////
    //// It would be great to fix this, and feel there should be a simple (ish) algorithm that would take into
    //// account whether the _loops_ cross (rather than just constituent curves) but I didn't figure that out yet
    //// and am instead just trying to avoid the scenario by shrinking the rectangle width slightly so its corner falls
    //// inside the circle...
    ////
    ////   [Test]
    ////   public void testPrecisionProblem()
    ////   {
    ////      engine.brep.Curve circle = new engine.brep.CircleCurve(new engine.XY(340.5690029832473, -103.41524432252388), 10.0,
    ////            0.0, Math.PI * 2, engine.brep.CircleCurve.RotationDirection.Forwards);
    ////
    ////      List<engine.brep.Curve> alc1 = new List<>();
    ////      alc1.add(circle);
    ////
    ////      engine.brep.Curve l1 = new engine.brep.LineCurve(new engine.XY(345.5653898846735, -112.07758337910997),
    ////            new engine.XY(-0.8662339056586087, -0.49963869014261947),
    ////            0.0, 122.2096167831618);
    ////      engine.brep.Curve l2 = new engine.brep.LineCurve(new engine.XY(239.70327622955338, -173.13823623148042),
    ////            new engine.XY(-0.49963869014261947, 0.8662339056586087),
    ////            0.0, 20.0);
    ////      engine.brep.Curve l3 = new engine.brep.LineCurve(new engine.XY(229.71050242670097, -155.81355811830824),
    ////            new engine.XY(0.8662339056586087, 0.49963869014261947),
    ////            0.0, 122.2096167831618);
    ////      engine.brep.Curve l4 = new engine.brep.LineCurve(new engine.XY(335.5726160818211, -94.75290526593778),
    ////            new engine.XY(0.49963869014261947, -0.8662339056586087),
    ////            0, 20);
    ////
    ////      List alc2 = new List();
    ////      alc2.add(l1);
    ////      alc2.add(l2);
    ////      alc2.add(l3);
    ////      alc2.add(l4);
    ////
    ////      engine.brep.Intersector.splitCurvesAtIntersections(alc1, alc2, 1e-6);
    ////   }
}
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System;
using Assets.Generation.U;
using Assets.Generation.GeomRep;
using Assets.Extensions;
using System.Linq;

public class IntersectorTest
{
    private readonly Intersector m_intersector = new Intersector();
    
    class FakeCurve : Curve
    {
        public override float StartParam => throw new NotImplementedException();

        public override float EndParam => throw new NotImplementedException();

        public readonly String Name;
        
        public FakeCurve(String name)
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

        protected override float FindParamForPoint_Inner(Vector2 first)
        {
            return -100;
        }

        public override Curve CloneWithChangedParams(float start, float end)
        {
            return null;
        }

        public override Box2 BoundingArea
        {
            get => null;
        }

        public override Vector2 Tangent(float second)
        {
            return Vector2.zero;
        }

        public override Curve Merge(Curve c_after)
        {
            return null;
        }

        public override float Length
        {
            get => 0;
        }

        public override Vector2 Normal(float p)
        {
            return Vector2.zero;
        }
    
        public override Curve Reversed()
        {
            throw new NotImplementedException();
        }
        public override Tuple<IList<Curve>, IList<Curve>> SplitCoincidentCurves(Curve c2, float tol)
        {
            throw new NotImplementedException();
        }
    }

    [Test]
    public void TestBuildAnnotationChains()
    {
        List<Curve> curves = new List<Curve>();

        Curve ca = new FakeCurve("a");
        Curve cb = new FakeCurve("b");
        Curve cc = new FakeCurve("c");
        Curve cd = new FakeCurve("d");
        Curve ce = new FakeCurve("e");

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

        Assert.IsTrue(curves1[0].EndPos.Equals(curves1[1].StartPos, 1e-5f));
        Assert.IsTrue(curves1[1].EndPos.Equals(curves1[2].StartPos, 1e-5f));
        Assert.IsTrue(curves1[2].EndPos.Equals(curves1[0].StartPos, 1e-5f));
        Assert.IsTrue(curves2[0].EndPos.Equals(curves2[1].StartPos, 1e-5f));
        Assert.IsTrue(curves2[1].EndPos.Equals(curves2[2].StartPos, 1e-5f));
        Assert.IsTrue(curves2[2].EndPos.Equals(curves2[0].StartPos, 1e-5f));

        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[0].EndParam, curves1[1].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[1].EndParam, curves1[2].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[2].EndParam, curves1[0].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[0].EndParam, curves2[1].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[1].EndParam, curves2[2].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[2].EndParam, curves2[0].StartParam, 1e-5f));
    }

    [Test]
    public void TestSplitCurvesAtIntersections_TwoCirclesOnePoint()
    {
        // circles meet at one point
        Curve cc1 = new CircleCurve(new Vector2(), 1);
        Curve cc2 = new CircleCurve(new Vector2(2, 0), 1);

        List<Curve> curves1 = new List<Curve>();
        curves1.Add(cc1);

        List<Curve> curves2 = new List<Curve>();
        curves2.Add(cc2);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

        Assert.AreEqual(2, curves1.Count);
        Assert.AreEqual(2, curves2.Count);

        Assert.IsTrue(curves1[0].EndPos.Equals(curves1[1].StartPos, 1e-5f));
        Assert.IsTrue(curves1[1].EndPos.Equals(curves1[0].StartPos, 1e-5f));
        Assert.IsTrue(curves2[0].EndPos.Equals(curves2[1].StartPos, 1e-5f));
        Assert.IsTrue(curves2[1].EndPos.Equals(curves2[0].StartPos, 1e-5f));

        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[0].EndParam, curves1[1].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[1].EndParam, curves1[0].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[0].EndParam, curves2[1].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[1].EndParam, curves2[0].StartParam, 1e-5f));
    }

    [Test]
    public void TestSplitCurvesAtIntersections_SameCircleTwice()
    {
        // same circle twice
        Curve cc1 = new CircleCurve(new Vector2(), 1);
        Curve cc2 = new CircleCurve(new Vector2(), 1);

        List<Curve> curves1 = new List<Curve>();
        curves1.Add(cc1);

        List<Curve> curves2 = new List<Curve>();
        curves2.Add(cc2);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

        Assert.AreEqual(1, curves1.Count);
        Assert.AreEqual(1, curves2.Count);

        Assert.IsTrue(curves1[0].EndPos.Equals(curves1[0].StartPos, 1e-5f));
        Assert.IsTrue(curves2[0].EndPos.Equals(curves2[0].StartPos, 1e-5f));

        Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[0].EndParam, curves1[0].StartParam, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(curves2[0].EndParam, curves2[0].StartParam, 1e-5f));
    }

    [Test]
    public void TestSplitCurvesAtIntersections_OneCircleHitsBreakInOther()
    {
        // one circle hits existing break in other
        Curve cc1 = new CircleCurve(new Vector2(), 1);

        List<Curve> curves1 = new List<Curve>();
        curves1.Add(cc1);

        List<Curve> curves2 = new List<Curve>();

        for (float a = 0; a < Math.PI * 2; a += 0.1f)
        {
            Curve cc2 = new CircleCurve(new Vector2(Mathf.Sin(a), Mathf.Cos(a)), 1);

            curves2.Add(cc2);
        }

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

        for (int i = 0; i < curves1.Count; i++)
        {
            int next_i = (i + 1) % curves1.Count;
            Assert.IsTrue(Util.ClockAwareAngleCompare(curves1[i].EndParam, curves1[next_i].StartParam, 1e-5f));
            Assert.IsTrue(curves1[i].EndPos.Equals(curves1[next_i].StartPos, 1e-5f));
        }
    }

    [Test]
    public void TestFindSplices()
    {
        Curve cc1 = new CircleCurve(new Vector2(), 1);
        Curve cc2 = new CircleCurve(new Vector2(1, 0), 1);

        List<Curve> curves1 = new List<Curve>();
        curves1.Add(cc1);

        List<Curve> curves2 = new List<Curve>();
        curves2.Add(cc2);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

        Dictionary<Curve, Intersector.AnnotatedCurve> forward_annotations_map = new Dictionary<Curve, Intersector.AnnotatedCurve>();

        m_intersector.BuildAnnotationChains(curves1, 1,
              forward_annotations_map);

        m_intersector.BuildAnnotationChains(curves2, 2,
              forward_annotations_map);

        Dictionary<Curve, Intersector.Splice> endSpliceMap = new Dictionary<Curve, Intersector.Splice>();

        m_intersector.FindSplices(curves1, curves2,
              forward_annotations_map,
              endSpliceMap);

        // two splices, with two in and two out curves each
        Assert.AreEqual(4, endSpliceMap.Count);

        HashSet<Intersector.Splice> unique = new HashSet<Intersector.Splice>();
        unique.UnionWith(endSpliceMap.Values);

        Assert.AreEqual(2, unique.Count);

        foreach (Intersector.Splice s in unique)
        {
            HashSet<Intersector.AnnotatedCurve> l1fset = new HashSet<Intersector.AnnotatedCurve>();
            HashSet<Intersector.AnnotatedCurve> l2fset = new HashSet<Intersector.AnnotatedCurve>();

            Intersector.AnnotatedCurve acl1f = s.Loop1Out;
            Intersector.AnnotatedCurve acl2f = s.Loop2Out;

            for (int i = 0; i < 4; i++)
            {
                l1fset.Add(acl1f);
                l2fset.Add(acl2f);

                acl1f = acl1f.Next;
                acl2f = acl2f.Next;
            }

            // although we stepped four times, the loops are of length 3 and we
            // shouldn't have found any more AnnotationCurves
            Assert.AreEqual(3, l1fset.Count);
            Assert.AreEqual(3, l2fset.Count);

            // loops of AnnotationCurves should be unique
            Assert.IsFalse(l1fset.Overlaps(l2fset));

            HashSet<Curve> l1fcset = new HashSet<Curve>(l1fset.Select(x => x.Curve));
            HashSet<Curve> l2fcset = new HashSet<Curve>(l2fset.Select(x => x.Curve));

            // and l1 and l2 don't contain any of the same curves
            Assert.IsFalse(l1fcset.Overlaps(l2fcset));
        }
    }

    [Test]
    public void TestTryFindIntersections()
    {
        // one circle, expect 1, 0
        {
            CircleCurve cc = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = new HashSet<Curve>();
            all_curves.Add(cc);

            HashSet<Vector2> curve_joints = new HashSet<Vector2>();
            curve_joints.Add(cc.StartPos);

            List<Tuple<Curve, int>> ret =
                  m_intersector.TryFindIntersections(
                        new Vector2(0, -5),
                        all_curves,
                        curve_joints,
                        10, 1e-5f,
                        new ClRand(1)
                  );

            Assert.IsNotNull(ret);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(cc, ret[0].Item1);
            Assert.AreEqual(cc, ret[1].Item1);
            Assert.AreEqual(1, ret[0].Item2);
            Assert.AreEqual(0, ret[1].Item2);
        }

        // two concentric circles, expect 1, 2, 1, 0
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 3);

            HashSet<Curve> all_curves = new HashSet<Curve>();
            all_curves.Add(cc1);
            all_curves.Add(cc2);

            HashSet<Vector2> curve_joints = new HashSet<Vector2>();
            curve_joints.Add(cc1.StartPos);
            curve_joints.Add(cc2.StartPos);

            List<Tuple<Curve, int>> ret =
                  m_intersector.TryFindIntersections(
                        new Vector2(0, 0),  // use centre to force hitting both circles
                        all_curves,
                        curve_joints,
                        10, 1e-5f,
                        new ClRand(1)
                  );

            Assert.IsNotNull(ret);
            Assert.AreEqual(4, ret.Count);
            Assert.AreEqual(cc1, ret[0].Item1);
            Assert.AreEqual(cc2, ret[1].Item1);
            Assert.AreEqual(cc2, ret[2].Item1);
            Assert.AreEqual(cc1, ret[3].Item1);
            Assert.AreEqual(1, ret[0].Item2);
            Assert.AreEqual(2, ret[1].Item2);
            Assert.AreEqual(1, ret[2].Item2);
            Assert.AreEqual(0, ret[3].Item2);
        }

        // two concentric circles, inner one -ve, expect 1, 0, 1, 0
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 3, RotationDirection.Reverse);

            HashSet<Curve> all_curves = new HashSet<Curve>();
            all_curves.Add(cc1);
            all_curves.Add(cc2);

            HashSet<Vector2> curve_joints = new HashSet<Vector2>();
            curve_joints.Add(cc1.StartPos);
            curve_joints.Add(cc2.StartPos);

            List<Tuple<Curve, int>> ret =
                  m_intersector.TryFindIntersections(
                        new Vector2(0, 0),  // use centre to force hitting both circles
                        all_curves,
                        curve_joints,
                        10, 1e-5f,
                        new ClRand(1)
                  );

            Assert.IsNotNull(ret);
            Assert.AreEqual(4, ret.Count);
            Assert.AreEqual(cc1, ret[0].Item1);
            Assert.AreEqual(cc2, ret[1].Item1);
            Assert.AreEqual(cc2, ret[2].Item1);
            Assert.AreEqual(cc1, ret[3].Item1);
            Assert.AreEqual(1, (int)ret[0].Item2);
            Assert.AreEqual(0, (int)ret[1].Item2);
            Assert.AreEqual(1, (int)ret[2].Item2);
            Assert.AreEqual(0, (int)ret[3].Item2);
        }
    }

    [Test]
    public void TestTryFindCurveIntersections()
    {
        // this was in the above, no idea why I went to the lower level routine in there
        // but I need to do that now anyway...

        // one circle, built from two half-circles, should still work
        // expect 1, 0
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5, 0, Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 5, Mathf.PI, 2 * Mathf.PI);

            HashSet<Curve> all_curves = new HashSet<Curve>();
            all_curves.Add(cc1);
            all_curves.Add(cc2);

            LineCurve lc = new LineCurve(new Vector2(-10, 0), new Vector2(1, 0), 20);

            List<Tuple<Curve, int>> ret =
                  m_intersector.TryFindCurveIntersections(
                        lc,
                        all_curves);

            Assert.IsNotNull(ret);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(cc2, ret[0].Item1);
            Assert.AreEqual(cc1, ret[1].Item1);
            Assert.AreEqual(1, (int)ret[0].Item2);
            Assert.AreEqual(0, (int)ret[1].Item2);
        }

        // miss the circle, expect null
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = new HashSet<Curve>();
            all_curves.Add(cc1);

            LineCurve lc = new LineCurve(new Vector2(-10, 0), new Vector2(0, 1), 20);

            List<Tuple<Curve, int>> ret =
                  m_intersector.TryFindCurveIntersections(
                        lc,
                        all_curves);

            Assert.IsNull(ret);
        }

        // clip the circle, to simplify the analysis we disregard these, expect null
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = new HashSet<Curve>();
            all_curves.Add(cc1);

            LineCurve lc = new LineCurve(new Vector2(-5, -5), new Vector2(0, 1), 20);

            List<Tuple<Curve, int>> ret =
                  m_intersector.TryFindCurveIntersections(
                        lc,
                        all_curves);

            Assert.IsNull(ret);
        }
    }

    private static void CheckLoop(Loop l, int exp_size)
    {
        Assert.AreEqual(exp_size, l.NumCurves);

        Vector2 prev_end = l.Curves[l.NumCurves - 1].EndPos;

        foreach (Curve c in l.Curves)
        {
            Assert.IsTrue(prev_end.Equals(c.StartPos, 1e-5f));
            prev_end = c.EndPos;

            Assert.IsTrue(c is CircleCurve);
        }
    }

    [Test]
    public void TestUnion()
    {
        // nothing union nothing should equal nothing
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsTrue(ret.Count == 0);
        }

        // something union nothing should equal something
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(ls1, ret);
        }

        // nothing union something should equal something
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l2 = new Loop(new CircleCurve(new Vector2(), 1));
            ls2.Add(l2);

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(ls2, ret);
        }

        // union of two identical things should equal either one of them
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop(new CircleCurve(new Vector2(), 1));
            ls2.Add(l2);

            // paranoia
            Assert.AreEqual(ls1, ls2);

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(ls1, ret);
        }

        // union of two overlapping circles should be one two-part curve
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop(new CircleCurve(new Vector2(1, 0), 1));
            ls2.Add(l2);

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(2, ret[0].NumCurves);
            Curve c1 = ret[0].Curves[0];
            Curve c2 = ret[0].Curves[1];
            Assert.IsTrue(c1 is CircleCurve);
            Assert.IsTrue(c2 is CircleCurve);

            CircleCurve cc1 = (CircleCurve)c1;
            CircleCurve cc2 = (CircleCurve)c2;

            // same radii
            Assert.AreEqual(1, cc1.Radius, 1e-5f);
            Assert.AreEqual(1, cc2.Radius, 1e-5f);

            // same direction
            Assert.AreEqual(RotationDirection.Forwards, cc1.Rotation);
            Assert.AreEqual(RotationDirection.Forwards, cc2.Rotation);

            // joined end-to-end
            Assert.IsTrue(cc1.StartPos.Equals(cc2.EndPos, 1e-5f));
            Assert.IsTrue(cc2.StartPos.Equals(cc1.EndPos, 1e-5f));

            CircleCurve left = cc1.Position.x < cc2.Position.x ? cc1 : cc2;
            CircleCurve right = cc1.Position.x > cc2.Position.x ? cc1 : cc2;

            Assert.AreEqual(new Vector2(0, 0), left.Position);
            Assert.AreEqual(new Vector2(1, 0), right.Position);

            Assert.AreEqual(Math.PI * 2 * 5 / 12, left.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 13 / 12, left.EndParam, 1e-5f);

            Assert.AreEqual(Math.PI * 2 * 11 / 12, right.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 19 / 12, right.EndParam, 1e-5f);
        }

        // union of two overlapping circles with holes in
        // should be one two-part curve around outside and two two-part curves in the interior
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1a = new Loop(new CircleCurve(new Vector2(), 1));
            Loop l1b = new Loop(new CircleCurve(new Vector2(), 0.3f, RotationDirection.Reverse));
            ls1.Add(l1a);
            ls1.Add(l1b);

            Loop l2a = new Loop(new CircleCurve(new Vector2(1, 0), 1));
            Loop l2b = new Loop(new CircleCurve(new Vector2(1, 0), 0.3f, RotationDirection.Reverse));
            ls2.Add(l2a);
            ls2.Add(l2b);

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(3, ret.Count);

            CheckLoop(ret[0], 2);
            CheckLoop(ret[1], 2);
            CheckLoop(ret[2], 2);
        }

        // osculating circles, outside each other
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop(new CircleCurve(new Vector2(2, 0), 1));
            ls2.Add(l2);

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(2, ret[0].NumCurves);
            Curve c1 = ret[0].Curves[0];
            Curve c2 = ret[0].Curves[1];
            Assert.IsTrue(c1 is CircleCurve);
            Assert.IsTrue(c2 is CircleCurve);

            CircleCurve cc1 = (CircleCurve)c1;
            CircleCurve cc2 = (CircleCurve)c2;

            // same radii
            Assert.AreEqual(1, cc1.Radius, 1e-5f);
            Assert.AreEqual(1, cc2.Radius, 1e-5f);

            // same direction
            Assert.AreEqual(RotationDirection.Forwards, cc1.Rotation);
            Assert.AreEqual(RotationDirection.Forwards, cc2.Rotation);

            // joined end-to-end
            Assert.IsTrue(cc1.StartPos.Equals(cc2.EndPos, 1e-5f));
            Assert.IsTrue(cc2.StartPos.Equals(cc1.EndPos, 1e-5f));

            CircleCurve left = cc1.Position.x < cc2.Position.x ? cc1 : cc2;
            CircleCurve right = cc1.Position.x > cc2.Position.x ? cc1 : cc2;

            Assert.AreEqual(new Vector2(0, 0), left.Position);
            Assert.AreEqual(new Vector2(2, 0), right.Position);

            Assert.AreEqual(Math.PI * 2 * 3 / 12, left.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 15 / 12, left.EndParam, 1e-5f);

            Assert.AreEqual(Math.PI * 2 * 9 / 12, right.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 21 / 12, right.EndParam, 1e-5f);
        }

        // osculating circles, outside each other
        // other way around
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop(new CircleCurve(new Vector2(2, 0), 1));
            ls2.Add(l2);

            LoopSet ret = m_intersector.Union(ls2, ls1, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(2, ret[0].NumCurves);
            Curve c1 = ret[0].Curves[0];
            Curve c2 = ret[0].Curves[1];
            Assert.IsTrue(c1 is CircleCurve);
            Assert.IsTrue(c2 is CircleCurve);

            CircleCurve cc1 = (CircleCurve)c1;
            CircleCurve cc2 = (CircleCurve)c2;

            // same radii
            Assert.AreEqual(1, cc1.Radius, 1e-5f);
            Assert.AreEqual(1, cc2.Radius, 1e-5f);

            // same direction
            Assert.AreEqual(RotationDirection.Forwards, cc1.Rotation);
            Assert.AreEqual(RotationDirection.Forwards, cc2.Rotation);

            // joined end-to-end
            Assert.IsTrue(cc1.StartPos.Equals(cc2.EndPos, 1e-5f));
            Assert.IsTrue(cc2.StartPos.Equals(cc1.EndPos, 1e-5f));

            CircleCurve left = cc1.Position.x < cc2.Position.x ? cc1 : cc2;
            CircleCurve right = cc1.Position.x > cc2.Position.x ? cc1 : cc2;

            Assert.AreEqual(new Vector2(0, 0), left.Position);
            Assert.AreEqual(new Vector2(2, 0), right.Position);

            Assert.AreEqual(Math.PI * 2 * 3 / 12, left.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 15 / 12, left.EndParam, 1e-5f);

            Assert.AreEqual(Math.PI * 2 * 9 / 12, right.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 21 / 12, right.EndParam, 1e-5f);
        }

        // osculating circles, one smaller, reversed and inside the other
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop(new CircleCurve(new Vector2(0.5f, 0), 0.5f, RotationDirection.Reverse));
            ls2.Add(l2);

            LoopSet ret = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(2, ret[0].NumCurves);
            Curve c1 = ret[0].Curves[0];
            Curve c2 = ret[0].Curves[1];
            Assert.IsTrue(c1 is CircleCurve);
            Assert.IsTrue(c2 is CircleCurve);

            CircleCurve cc1 = (CircleCurve)c1;
            CircleCurve cc2 = (CircleCurve)c2;

            // joined end-to-end
            Assert.IsTrue(cc1.StartPos.Equals(cc2.EndPos, 1e-5f));
            Assert.IsTrue(cc2.StartPos.Equals(cc1.EndPos, 1e-5f));

            CircleCurve left = cc1.Position.x < cc2.Position.x ? cc1 : cc2;
            CircleCurve right = cc1.Position.x > cc2.Position.x ? cc1 : cc2;

            // same radii
            Assert.AreEqual(1, left.Radius, 1e-5f);
            Assert.AreEqual(0.5, right.Radius, 1e-5f);

            // same direction
            Assert.AreEqual(RotationDirection.Forwards, left.Rotation);
            Assert.AreEqual(RotationDirection.Reverse, right.Rotation);

            Assert.AreEqual(new Vector2(0, 0), left.Position);
            Assert.AreEqual(new Vector2(0.5f, 0), right.Position);

            Assert.AreEqual(Math.PI * 2 * 3 / 12, left.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 15 / 12, left.EndParam, 1e-5f);

            Assert.AreEqual(Math.PI * 2 * 9 / 12, right.StartParam, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 21 / 12, right.EndParam, 1e-5f);
        }
    }

    [Test]
    public void TestAnnotatedCurve()
    {
        Curve c1 = new CircleCurve(new Vector2(), 1);
        Curve c2 = new CircleCurve(new Vector2(), 2);

        Intersector.AnnotatedCurve ac1 = new Intersector.AnnotatedCurve(c1, 1);
        Intersector.AnnotatedCurve ac1b = new Intersector.AnnotatedCurve(c1, 1);
        Intersector.AnnotatedCurve ac2 = new Intersector.AnnotatedCurve(c2, 1);

        Assert.AreEqual(c1.GetHashCode(), ac1.GetHashCode());

        Assert.IsTrue(ac1.Equals(ac1b));
        Assert.IsFalse(ac1.Equals(ac2));
        Assert.IsFalse(ac1.Equals(0));
    }

    class IntersectorDummy1 : Intersector
    {
        public override bool ExtractInternalCurves(
            float tol, ClRand ClRand,
            Dictionary<Curve, AnnotatedCurve> forward_annotations_map, HashSet<Curve> all_curves,
            HashSet<AnnotatedCurve> open, HashSet<Vector2> curve_joints, float diameter,
            UnionType type)
        {
            return false;
        }
    }

    class IntersectorDummy2 : Intersector
    {
        public override List<Tuple<Curve, int>> TryFindIntersections(
            Vector2 mid_point,
            HashSet<Curve> all_curves,
            HashSet<Vector2> curve_joints,
            float diameter, float tol,
            ClRand ClRand)
        {
            return null;
        }
    }

    class IntersectorDummy3 : Intersector
    {
        public override bool LineClearsPoints(LineCurve lc, HashSet<Vector2> curve_joints, float tol)
        {
            return false;
        }
    }

    [Test]
    public void TestUnion_CornerCases()
    {
        // union of two identical objects is one of the objects
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);
            ls2.Add(l1);

            Intersector i = new Intersector();

            LoopSet ret = i.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.AreEqual(ret, ls1);
        }

        // if extractInternalCurves fails, we bail...
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);
            Loop l2 = new Loop(new CircleCurve(new Vector2(1, 0), 1));
            ls2.Add(l2);

            Intersector i = new IntersectorDummy1();

            LoopSet ret = i.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNull(ret);
        }

        // if tryFindIntersections fails, we bail
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop(new CircleCurve(new Vector2(0.5f, 0), 1));
            ls2.Add(l2);

            Intersector i = new IntersectorDummy2();

            LoopSet ret = i.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNull(ret);
        }

        // if tryFindIntersections fails, we bail
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop(new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop(new CircleCurve(new Vector2(0.5f, 0), 1));
            ls2.Add(l2);

            Intersector i = new IntersectorDummy3();

            LoopSet ret = i.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNull(ret);
        }
    }

    [Test]
    public void TestLineClearsPoints()
    {
        LineCurve lc1 = new LineCurve(new Vector2(), new Vector2(1, 0), 10);
        LineCurve lc2 = new LineCurve(new Vector2(), new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), 10);

        {
            HashSet<Vector2> hs = new HashSet<Vector2>();
            hs.Add(new Vector2(1, 1));

            Assert.IsTrue(m_intersector.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsFalse(m_intersector.LineClearsPoints(lc2, hs, 1e-5f));
        }

        {
            HashSet<Vector2> hs = new HashSet<Vector2>();
            hs.Add(new Vector2(0, 0));

            Assert.IsFalse(m_intersector.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsFalse(m_intersector.LineClearsPoints(lc2, hs, 1e-5f));
        }

        {
            HashSet<Vector2> hs = new HashSet<Vector2>();
            hs.Add(new Vector2(2, 0));

            Assert.IsFalse(m_intersector.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsTrue(m_intersector.LineClearsPoints(lc2, hs, 1e-5f));
        }
    }

    [Test]
    public void TestRandomUnions()
    {
        const int NumTests = 1000;
        const int NumShapes = 5;

        for(int i = 0; i < NumTests; i++)
        {
            // let us jump straight to a given test
            ClRand test_rand = new ClRand(i);

            LoopSet merged = new LoopSet();

            for (int j = 0; j < NumShapes; j++)
            {
                LoopSet ls2 = RandShapeLoop(test_rand);

                // point here is to run all the Unions internal logic/asserts
                merged = m_intersector.Union(merged, ls2, 1e-5f, new ClRand(1));
            }
        }
    }

    private LoopSet RandShapeLoop(ClRand test_rand)
    {
        LoopSet ret = new LoopSet();

        if (test_rand.Nextfloat() > 0.5f)
        {
            ret.Add(new Loop(new CircleCurve(
                test_rand.Nextpos(0, 10),
                test_rand.Nextfloat() * 2 + 0.1f,
                test_rand.Nextfloat() > 0.5f ? RotationDirection.Forwards : RotationDirection.Reverse)));
        }
        else
        {
            Vector2 p1 = test_rand.Nextpos(0, 10);
            Vector2 p2 = test_rand.Nextpos(0, 10);
            Vector2 p3 = test_rand.Nextpos(0, 10);

            // triangles cannot be self-intersecting
            Loop loop = new Loop(new List<Curve>{
                LineCurve.MakeFromPoints(p1, p2),
                LineCurve.MakeFromPoints(p2, p3),
                LineCurve.MakeFromPoints(p3, p1),
            });

            ret.Add(loop);
        }

        return ret;
    }

    //// This one asserts because it somehow tries to make a discontinuous loop
    //// but, pragmatically, I don't need this test yet
    ////   [Test]
    ////   public void testUnion_ManyPoints()
    ////   {
    ////      LoopSet ls1 = new LoopSet();
    ////
    ////      ClRand r = new ClRand(1);
    ////
    ////      for(int i = 0; i < 1000; i++)
    ////      {
    ////         LoopSet ls2 = new LoopSet();
    ////
    ////         Loop l2 = new Loop(new CircleCurve(new XY(r.nextfloat(), r.nextfloat()), .1));
    ////         ls2.Add(l2);
    ////
    ////         // try to make sure we have some lines hit some points
    ////         // (to his that return false in LineClearsPoints)
    ////         ls1 = m_intersector.Union(ls1, ls2, 1e-2, r);
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
    ////            0.0, Math.PI * 2, engine.brep.RotationDirection.Forwards);
    ////
    ////      List<engine.brep.Curve> alc1 = new List<Curve>();
    ////      alc1.Add(circle);
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
    ////      alc2.Add(l1);
    ////      alc2.Add(l2);
    ////      alc2.Add(l3);
    ////      alc2.Add(l4);
    ////
    ////      engine.brep.Intersector.splitCurvesAtIntersections(alc1, alc2, 1e-5f);
    ////   }
}
using Assets.Extensions;
using Assets.Generation.GeomRep;
using Assets.Generation.U;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IntersectorTest
{
    private readonly Intersector m_intersector = new Intersector();

    class FakeCurve : Curve
    {
        public readonly String Name;

        public FakeCurve(String name)
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

        public override bool Equals(Curve c, float tol)
        {
            // we're only going to want to compare unique modelling for the test cases
            return ReferenceEquals(this, c);
        }

        protected override float FindParamForPoint_Inner(Vector2 first)
        {
            return -100;
        }

        public override Curve CloneWithChangedExtents(float start, float end)
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

        public override bool SameSupercurve(Curve curve, float tol)
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

        Dictionary<Curve, Intersector.AnnotatedCurve> forward_annotations_map = Intersector.MakeForwardAnnotationsMap();

        m_intersector.BuildAnnotationChains(curves, 1, forward_annotations_map);

        foreach (Curve c in curves)
        {
            Assert.IsNotNull(forward_annotations_map[c]);
            Assert.IsNotNull(forward_annotations_map[c]);
            Assert.AreEqual(1, forward_annotations_map[c].LoopNumber);
        }
    }

    [Test]
    public void TestSplitCurvesAtIntersections_TwoCirclesTwoPoints()
    {
        // circles meet at two points
        Curve cc1 = new CircleCurve(new Vector2(), 1);
        Curve cc2 = new CircleCurve(new Vector2(1, 0), 1);

        List<Curve> curves1 = new List<Curve>
        {
            cc1
        };

        List<Curve> curves2 = new List<Curve>
        {
            cc2
        };

        var endSpliceMap = Intersector.MakeEndSpliceMap();

        m_intersector.SetupInitialSplices(curves1, endSpliceMap);
        m_intersector.SetupInitialSplices(curves2, endSpliceMap);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

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

        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves1[0]).AngleRange.End, ((CircleCurve)curves1[1]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves1[1]).AngleRange.End, ((CircleCurve)curves1[2]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves1[2]).AngleRange.End, ((CircleCurve)curves1[0]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves2[0]).AngleRange.End, ((CircleCurve)curves2[1]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves2[1]).AngleRange.End, ((CircleCurve)curves2[2]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves2[2]).AngleRange.End, ((CircleCurve)curves2[0]).AngleRange.Start, 1e-5f));
    }

    [Test]
    public void TestSplitCurvesAtIntersections_TwoCirclesOnePoint()
    {
        // circles meet at one point
        Curve cc1 = new CircleCurve(new Vector2(), 1);
        Curve cc2 = new CircleCurve(new Vector2(2, 0), 1);

        List<Curve> curves1 = new List<Curve>
        {
            cc1
        };

        List<Curve> curves2 = new List<Curve>
        {
            cc2
        };

        var endSpliceMap = Intersector.MakeEndSpliceMap();

        m_intersector.SetupInitialSplices(curves1, endSpliceMap);
        m_intersector.SetupInitialSplices(curves2, endSpliceMap);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

        Assert.AreEqual(2, curves1.Count);
        Assert.AreEqual(2, curves2.Count);

        Assert.IsTrue(curves1[0].EndPos.Equals(curves1[1].StartPos, 1e-5f));
        Assert.IsTrue(curves1[1].EndPos.Equals(curves1[0].StartPos, 1e-5f));
        Assert.IsTrue(curves2[0].EndPos.Equals(curves2[1].StartPos, 1e-5f));
        Assert.IsTrue(curves2[1].EndPos.Equals(curves2[0].StartPos, 1e-5f));

        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves1[0]).AngleRange.End, ((CircleCurve)curves1[1]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves1[1]).AngleRange.End, ((CircleCurve)curves1[0]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves2[0]).AngleRange.End, ((CircleCurve)curves2[1]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves2[1]).AngleRange.End, ((CircleCurve)curves2[0]).AngleRange.Start, 1e-5f));
    }

    [Test]
    public void TestSplitCurvesAtIntersections_SameCircleTwice()
    {
        // same circle twice
        Curve cc1 = new CircleCurve(new Vector2(), 1);
        Curve cc2 = new CircleCurve(new Vector2(), 1);

        List<Curve> curves1 = new List<Curve>
        {
            cc1
        };

        List<Curve> curves2 = new List<Curve>
        {
            cc2
        };

        var endSpliceMap = Intersector.MakeEndSpliceMap();

        m_intersector.SetupInitialSplices(curves1, endSpliceMap);
        m_intersector.SetupInitialSplices(curves2, endSpliceMap);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

        Assert.AreEqual(1, curves1.Count);
        Assert.AreEqual(1, curves2.Count);

        Assert.IsTrue(curves1[0].EndPos.Equals(curves1[0].StartPos, 1e-5f));
        Assert.IsTrue(curves2[0].EndPos.Equals(curves2[0].StartPos, 1e-5f));

        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves1[0]).AngleRange.End, ((CircleCurve)curves1[0]).AngleRange.Start, 1e-5f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(((CircleCurve)curves2[0]).AngleRange.End, ((CircleCurve)curves2[0]).AngleRange.Start, 1e-5f));
    }

    [Test]
    public void TestSplitCurvesAtIntersections_OneCircleHitsBreakInOther()
    {
        // one circle hits existing break in other
        Curve cc1 = new CircleCurve(new Vector2(), 1);

        List<Curve> curves1 = new List<Curve>
        {
            cc1
        };

        Curve cc2 = new CircleCurve(new Vector2(0, 2), 1);

        List<Curve> curves2 = new List<Curve>
        {
            cc2
        };

        var endSpliceMap = Intersector.MakeEndSpliceMap();

        m_intersector.SetupInitialSplices(curves1, endSpliceMap);
        m_intersector.SetupInitialSplices(curves2, endSpliceMap);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

        Assert.AreEqual(1, curves1.Count);
        Assert.AreEqual(2, curves2.Count);
    }

    [Test]
    public void TestSplitCurvesAtIntersections_Flower()
    {
        // one circle hits existing break in other
        Curve cc1 = new CircleCurve(new Vector2(), 1);

        List<Curve> curves1 = new List<Curve>
        {
            cc1
        };

        List<Curve> curves2 = new List<Curve>();

        for (int i = 0; i < 6; i++)
        {
            float a = i * Mathf.PI / 3;

            Curve cc2 = new CircleCurve(new Vector2(Mathf.Sin(a), Mathf.Cos(a)), 1);

            curves2.Add(cc2);
        }

        var endSpliceMap = Intersector.MakeEndSpliceMap();

        m_intersector.SetupInitialSplices(curves1, endSpliceMap);
        m_intersector.SetupInitialSplices(curves2, endSpliceMap);

        m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

        Assert.AreEqual(6, curves1.Count);

        for (int i = 0; i < curves1.Count; i++)
        {
            int next_i = (i + 1) % curves1.Count;
            Assert.IsTrue(Util.ClockAwareAngleCompare(
                ((CircleCurve)curves1[i]).AngleRange.End,
                ((CircleCurve)curves1[next_i]).AngleRange.Start, 1e-5f));
            Assert.IsTrue(curves1[i].EndPos.Equals(curves1[next_i].StartPos, 1e-5f));
        }
    }

    [Test]
    public void TestTryFindIntersections()
    {
        // one circle, expect 1, 0
        {
            CircleCurve cc = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(cc);

            HashSet<Vector2> curve_joints = new HashSet<Vector2>
            {
                cc.StartPos
            };

            var ret = m_intersector.TryFindIntersections(
                        cc,
                        all_curves,
                        curve_joints,
                        10, 1e-5f,
                        new ClRand(1)
                  );

            Assert.IsNotNull(ret);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(cc, ret[0].Curve);
            Assert.AreEqual(cc, ret[1].Curve);
            Assert.AreEqual(1, ret[0].CrossingNumber);
            Assert.AreEqual(0, ret[1].CrossingNumber);
            // we do not know what angle it scanned at
            // but the first intersection should be +ve and the second negative
            Assert.IsTrue(ret[0].DotProduct > 0);
            Assert.IsTrue(ret[1].DotProduct < 0);
            // and the first after the second
            Assert.IsTrue(ret[1].Distance >= ret[0].Distance);
        }

        // two concentric circles, expect 1, 2, 1, 0
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 3);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(new List<Curve>
                {
                    cc1,
                    cc2
                }
            );

            HashSet<Vector2> curve_joints = new HashSet<Vector2>
            {
                cc1.StartPos,
                cc2.StartPos
            };

            var ret =
                  m_intersector.TryFindIntersections(
                        cc2,
                        all_curves,
                        curve_joints,
                        10, 1e-5f,
                        new ClRand(1)
                  );

            Assert.IsNotNull(ret);
            Assert.AreEqual(4, ret.Count);
            Assert.AreEqual(cc1, ret[0].Curve);
            Assert.AreEqual(cc2, ret[1].Curve);
            Assert.AreEqual(cc2, ret[2].Curve);
            Assert.AreEqual(cc1, ret[3].Curve);
            Assert.AreEqual(1, ret[0].CrossingNumber);
            Assert.AreEqual(2, ret[1].CrossingNumber);
            Assert.AreEqual(1, ret[2].CrossingNumber);
            Assert.AreEqual(0, ret[3].CrossingNumber);
            Assert.IsTrue(ret[0].DotProduct > 0);
            Assert.IsTrue(ret[1].DotProduct > 0);
            Assert.IsTrue(ret[2].DotProduct < 0);
            Assert.IsTrue(ret[3].DotProduct < 0);
            Assert.IsTrue(ret[1].Distance >= ret[0].Distance);
            Assert.IsTrue(ret[2].Distance >= ret[1].Distance);
            Assert.IsTrue(ret[3].Distance >= ret[2].Distance);
        }

        // two concentric circles, inner one -ve, expect 1, 0, 1, 0
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 3, RotationDirection.Reverse);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(new List<Curve>
                {
                    cc1,
                    cc2
                }
            );

            HashSet<Vector2> curve_joints = new HashSet<Vector2>
            {
                cc1.StartPos,
                cc2.StartPos
            };

            var ret =
                  m_intersector.TryFindIntersections(
                        cc2,
                        all_curves,
                        curve_joints,
                        10, 1e-5f,
                        new ClRand(1)
                  );

            Assert.IsNotNull(ret);
            Assert.AreEqual(4, ret.Count);
            Assert.AreEqual(cc1, ret[0].Curve);
            Assert.AreEqual(cc2, ret[1].Curve);
            Assert.AreEqual(cc2, ret[2].Curve);
            Assert.AreEqual(cc1, ret[3].Curve);
            Assert.AreEqual(1, ret[0].CrossingNumber);
            Assert.AreEqual(0, ret[1].CrossingNumber);
            Assert.AreEqual(1, ret[2].CrossingNumber);
            Assert.AreEqual(0, ret[3].CrossingNumber);
            Assert.IsTrue(ret[0].DotProduct > 0);
            Assert.IsTrue(ret[1].DotProduct < 0);
            Assert.IsTrue(ret[2].DotProduct > 0);
            Assert.IsTrue(ret[3].DotProduct < 0);
            Assert.IsTrue(ret[1].Distance >= ret[0].Distance);
            Assert.IsTrue(ret[2].Distance >= ret[1].Distance);
            Assert.IsTrue(ret[3].Distance >= ret[2].Distance);
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

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(new List<Curve>
                {
                    cc1,
                    cc2
                }
            );

            LineCurve lc = new LineCurve(new Vector2(-10, 0), new Vector2(1, 0), 20);

            var ret =
                  m_intersector.TryFindCurveIntersections(
                        lc,
                        all_curves);

            Assert.IsNotNull(ret);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(cc2, ret[0].Curve);
            Assert.AreEqual(cc1, ret[1].Curve);
            Assert.AreEqual(1, ret[0].CrossingNumber);
            Assert.AreEqual(0, ret[1].CrossingNumber);
            Assert.AreEqual(5, ret[0].Distance, 1e-4f);
            Assert.AreEqual(15, ret[1].Distance, 1e-4f);
        }

        // miss the circle, expect null
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(new List<Curve>
                {
                    cc1
                }
            );

            LineCurve lc = new LineCurve(new Vector2(-10, 0), new Vector2(0, 1), 20);

            var ret =
                  m_intersector.TryFindCurveIntersections(
                        lc,
                        all_curves);

            Assert.IsNull(ret);
        }

        // clip the circle, to simplify the analysis we disregard these, expect null
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(new List<Curve>
                {
                    cc1
                }
            );

            LineCurve lc = new LineCurve(new Vector2(-5, -5), new Vector2(0, 1), 20);

            var ret =
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

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
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

            Loop l2 = new Loop("", new CircleCurve(new Vector2(), 1));
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

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop("", new CircleCurve(new Vector2(), 1));
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

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop("", new CircleCurve(new Vector2(1, 0), 1));
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

            Assert.AreEqual(Math.PI * 2 * 5 / 12, left.AngleRange.Start, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 13 / 12, left.AngleRange.End, 1e-5f);

            Assert.AreEqual(Math.PI * 2 * 11 / 12, right.AngleRange.Start, 1e-5f);
            Assert.AreEqual(Math.PI * 2 * 19 / 12, right.AngleRange.End, 1e-5f);
        }

        // union of two overlapping circles with holes in
        // should be one two-part curve around outside and two two-part curves in the interior
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1a = new Loop("", new CircleCurve(new Vector2(), 1));
            Loop l1b = new Loop("", new CircleCurve(new Vector2(), 0.3f, RotationDirection.Reverse));
            ls1.Add(l1a);
            ls1.Add(l1b);

            Loop l2a = new Loop("", new CircleCurve(new Vector2(1, 0), 1));
            Loop l2b = new Loop("", new CircleCurve(new Vector2(1, 0), 0.3f, RotationDirection.Reverse));
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

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop("", new CircleCurve(new Vector2(2, 0), 1));
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

            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 3 / 12, left.AngleRange.Start, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 15 / 12, left.AngleRange.End, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 9 / 12, right.AngleRange.Start, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 21 / 12, right.AngleRange.End, 1e-5f));
        }

        // osculating circles, outside each other
        // other way around
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop("", new CircleCurve(new Vector2(2, 0), 1));
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

            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 3 / 12, left.AngleRange.Start, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 15 / 12, left.AngleRange.End, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 9 / 12, right.AngleRange.Start, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 21 / 12, right.AngleRange.End, 1e-5f));
        }

        // osculating circles, one smaller, reversed and inside the other
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop("", new CircleCurve(new Vector2(0.5f, 0), 0.5f, RotationDirection.Reverse));
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

            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 3 / 12, left.AngleRange.Start, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 15 / 12, left.AngleRange.End, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 3 / 12, right.AngleRange.End, 1e-5f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(Mathf.PI * 2 * 15 / 12, right.AngleRange.Start, 1e-5f));
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
        public override bool RemoveUnwantedCurves(
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
        public override List<Interval> TryFindIntersections(
            Curve c,
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

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
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

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);
            Loop l2 = new Loop("", new CircleCurve(new Vector2(1, 0), 1));
            ls2.Add(l2);

            Intersector i = new IntersectorDummy1();

            LoopSet ret = i.Union(ls1, ls2, 1e-5f, new ClRand(1));

            Assert.IsNull(ret);
        }

        // if tryFindIntersections fails, we throw
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop("", new CircleCurve(new Vector2(0.5f, 0), 1));
            ls2.Add(l2);

            Intersector i = new IntersectorDummy2();

            bool caught = false;

            try
            {
                LoopSet ret = i.Union(ls1, ls2, 1e-5f, new ClRand(1));
            }
            catch (AnalysisFailedException)
            {
                caught = true;
            }

            Assert.IsTrue(caught);
        }

        // if tryFindIntersections fails (for a different reason), we throw
        {
            LoopSet ls1 = new LoopSet();
            LoopSet ls2 = new LoopSet();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            ls1.Add(l1);

            Loop l2 = new Loop("", new CircleCurve(new Vector2(0.5f, 0), 1));
            ls2.Add(l2);

            Intersector i = new IntersectorDummy3();

            bool caught = false;

            try
            {
                LoopSet ret = i.Union(ls1, ls2, 1e-5f, new ClRand(1));
            }
            catch (AnalysisFailedException)
            {
                caught = true;
            }

            Assert.IsTrue(caught);
        }

        {
            // iron cross has four possible outputs from one curve
            var pcentre = new Vector2();
            var p1tl = new Vector2(-1.0f, 0.5f);
            var p2tl = new Vector2(-0.5f, 1.0f);
            var p1tr = new Vector2(0.5f, 1.0f);
            var p2tr = new Vector2(1.0f, 0.5f);
            var p1bl = new Vector2(1.0f, -0.5f);
            var p2bl = new Vector2(0.5f, -1.0f);
            var p1br = new Vector2(-0.5f, -1.0f);
            var p2br = new Vector2(-1.0f, -0.5f);

            Loop l1 = Loop.MakePolygon(new List<Vector2>
            {
                pcentre, p1tl, p2tl,
            }, RotationDirection.Forwards);

            Loop l2 = Loop.MakePolygon(new List<Vector2>
            {
                pcentre, p1tr, p2tr,
            }, RotationDirection.Forwards);

            Loop l3 = Loop.MakePolygon(new List<Vector2>
            {
                pcentre, p1br, p2br,
            }, RotationDirection.Forwards);

            Loop l4 = Loop.MakePolygon(new List<Vector2>
            {
                pcentre, p1bl, p2bl,
            }, RotationDirection.Forwards);

            LoopSet ls1 = new LoopSet
            {
                l1, l2, l3, l4
            };

            m_intersector.Union(new LoopSet(), ls1, 1e-4f, new ClRand(1));
        }
    }

    [Test]
    public void TestLineClearsPoints()
    {
        LineCurve lc1 = new LineCurve(new Vector2(), new Vector2(1, 0), 10);
        LineCurve lc2 = new LineCurve(new Vector2(), new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), 10);

        {
            HashSet<Vector2> hs = new HashSet<Vector2>
            {
                new Vector2(1, 1)
            };

            Assert.IsTrue(m_intersector.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsFalse(m_intersector.LineClearsPoints(lc2, hs, 1e-5f));
        }

        {
            HashSet<Vector2> hs = new HashSet<Vector2>
            {
                new Vector2(0, 0)
            };

            Assert.IsFalse(m_intersector.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsFalse(m_intersector.LineClearsPoints(lc2, hs, 1e-5f));
        }

        {
            HashSet<Vector2> hs = new HashSet<Vector2>
            {
                new Vector2(2, 0)
            };

            Assert.IsFalse(m_intersector.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsTrue(m_intersector.LineClearsPoints(lc2, hs, 1e-5f));
        }
    }

    [Test]
    public void TestRandomUnions()
    {
        const int NumTests = 1000;
        const int NumShapes = 5;

        for (int i = 0; i < NumTests; i++)
        {
            // let us jump straight to a given test
            ClRand test_rand = new ClRand(i);

            LoopSet merged = new LoopSet();

            for (int j = 0; j < NumShapes; j++)
            {
                LoopSet ls2 = RandShapeLoop(test_rand);

                // point here is to run all the Unions internal logic/asserts
                merged = m_intersector.Union(merged, ls2, 1e-5f, new ClRand(1));
                // any Union output should be good as input to the next stage
                m_intersector.Union(merged, new LoopSet(), 1e-5f, new ClRand(1));
            }
        }
    }

    private LoopSet RandShapeLoop(ClRand test_rand)
    {
        LoopSet ret = new LoopSet();

        if (test_rand.Nextfloat() > 0.5f)
        {
            ret.Add(new Loop("", new CircleCurve(
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
            Loop loop = new Loop("", new List<Curve>{
                LineCurve.MakeFromPoints(p1, p2),
                LineCurve.MakeFromPoints(p2, p3),
                LineCurve.MakeFromPoints(p3, p1),
            });

            ret.Add(loop);
        }

        return ret;
    }

    [Test]
    public void PolygonRotationTest()
    {
        Vector2 p1 = new Vector2(0, 0);
        Vector2 p2 = new Vector2(0, 1);
        Vector2 p3 = new Vector2(1, 1);
        Vector2 p4 = new Vector2(1, 0);

        {
            // clockwise is forwards is a positive polygon
            Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3, p4 }, RotationDirection.Forwards);
            LoopSet ls1 = new LoopSet(l);

            LoopSet ret = m_intersector.Union(new LoopSet(), ls1,
                1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(ls1, ret);

            ret = m_intersector.Union(new LoopSet(), ls1,
                1e-5f, new ClRand(1), Intersector.UnionType.WantNegative);

            Assert.AreEqual(0, ret.Count);
        }

        {
            // anti-clockwise is reverse is a negative polygon
            Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3, p4 }, RotationDirection.Reverse);
            LoopSet ls1 = new LoopSet(l);

            LoopSet ret = m_intersector.Union(new LoopSet(), ls1,
                1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, ret.Count);

            ret = m_intersector.Union(new LoopSet(), ls1,
                1e-5f, new ClRand(1), Intersector.UnionType.WantNegative);

            Assert.AreEqual(ls1, ret);
        }
    }

    [Test]
    public void TestPolygonCircleCases()
    {
        // outside the circle, touching at 3 o'clock
        List<Vector2> p1 = new List<Vector2> {
            new Vector2(1, 0),
            new Vector2(2, 0),
            new Vector2(2, 1)
        };

        // inside the circle, touching at 3 o'clock
        List<Vector2> p2 = new List<Vector2> {
            new Vector2(1, 0),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0.5f)
        };

        // touching at 3 o'clock, crossing right across the circle and out the other side
        List<Vector2> p3 = new List<Vector2> {
            new Vector2(1, 0),
            new Vector2(-2, 0),
            new Vector2(-2, 0.5f)
        };

        // completely crossing the circle and out the other side
        List<Vector2> p4 = new List<Vector2> {
            new Vector2(2, 0),
            new Vector2(-2, 0),
            new Vector2(-2, 0.5f)
        };

        LoopSet p1f = new LoopSet(Loop.MakePolygon(p1, RotationDirection.Forwards));
        LoopSet p1r = new LoopSet(Loop.MakePolygon(p1, RotationDirection.Reverse));
        LoopSet p2f = new LoopSet(Loop.MakePolygon(p2, RotationDirection.Forwards));
        LoopSet p2r = new LoopSet(Loop.MakePolygon(p2, RotationDirection.Reverse));
        LoopSet p3f = new LoopSet(Loop.MakePolygon(p3, RotationDirection.Forwards));
        LoopSet p3r = new LoopSet(Loop.MakePolygon(p3, RotationDirection.Reverse));
        LoopSet p4f = new LoopSet(Loop.MakePolygon(p4, RotationDirection.Forwards));
        LoopSet p4r = new LoopSet(Loop.MakePolygon(p4, RotationDirection.Reverse));

        LoopSet circ = new LoopSet(new Loop("", new CircleCurve(new Vector2(0, 0), 1)));

        {
            LoopSet merged = m_intersector.Union(circ, p1f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(4, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(2, 1)), merged.GetBounds());
        }

        {
            LoopSet merged = m_intersector.Union(circ, p1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(1, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            LoopSet merged = m_intersector.Union(circ, p2f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(1, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            LoopSet merged = m_intersector.Union(circ, p2r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(4, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            LoopSet merged = m_intersector.Union(circ, p3f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(4, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-2, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            LoopSet merged = m_intersector.Union(circ, p3r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            // because our rule is, when two curves intersect, to change curve if possible, we get two pieces out here
            //          ____
            //         /    \ <-- c1
            //        /______\
            // c2 -->  ___---/
            //         \____/ <-- c1
            //       
            Assert.AreEqual(2, merged.Count);
            Assert.AreEqual(2, merged[0].Curves.Count);
            Assert.AreEqual(2, merged[1].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            LoopSet merged = m_intersector.Union(circ, p4f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(7, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-2, -1), new Vector2(2, 1)), merged.GetBounds());
        }

        {
            LoopSet merged = m_intersector.Union(circ, p4r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            // cut cleanly into two
            Assert.AreEqual(2, merged.Count);
            Assert.AreEqual(2, merged[0].Curves.Count);
            Assert.AreEqual(2, merged[1].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }
    }

    [Test]
    public void TestCoincidentLines()
    {
        {
            // simple degenerate polygon should disappear
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);

            LoopSet ls1 = new LoopSet(
                new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p1, p2),
                    LineCurve.MakeFromPoints(p2, p1),
                }));

            LoopSet ls1r = new LoopSet(
                new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p2, p1),
                    LineCurve.MakeFromPoints(p1, p2),
                }));

            // with the old implementation, one out of forwards and backwards on this should fail
            // (because the order of presenting the two curves will make it look like a zero-sized
            // +ve poly or a zero-sized -ve poly...)
            LoopSet merged = m_intersector.Union(new LoopSet(), ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);

            merged = m_intersector.Union(new LoopSet(), ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);
        }

        {
            // slightly more complex degenerate polygon should disappear
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);
            Vector2 p3 = new Vector2(2, 0);
            Vector2 p4 = new Vector2(3, 0);

            // p1 -> p3 -> p2 -> p4 -> p1

            LoopSet ls1 = new LoopSet(
                new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p1, p3),
                    LineCurve.MakeFromPoints(p3, p2),
                    LineCurve.MakeFromPoints(p2, p4),
                    LineCurve.MakeFromPoints(p4, p1),
                })
            );

            LoopSet ls1r = new LoopSet(
                ls1[0].Reversed()
            );

            // with the old implementation, one out of forwards and backwards on this should fail
            // (because the order of presenting the two curves will make it look like a zero-sized
            // +ve poly or a zero-sized -ve poly...)
            LoopSet merged = m_intersector.Union(new LoopSet(), ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);

            merged = m_intersector.Union(new LoopSet(), ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);
        }

        {
            // degenerate U should disappear

            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);
            Vector2 p3 = new Vector2(1, 1);
            Vector2 p4 = new Vector2(0, 1);

            // p1 -> p3 -> p2 -> p4 -> p3 -> p2 -> p1

            LoopSet ls1 = new LoopSet(
                new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p1, p2),
                    LineCurve.MakeFromPoints(p2, p3),
                    LineCurve.MakeFromPoints(p3, p4),
                    LineCurve.MakeFromPoints(p4, p3),
                    LineCurve.MakeFromPoints(p3, p2),
                    LineCurve.MakeFromPoints(p2, p1),
                })
            );

            LoopSet ls1r = new LoopSet(
                ls1[0].Reversed()
            );

            LoopSet merged = m_intersector.Union(new LoopSet(), ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);

            merged = m_intersector.Union(new LoopSet(), ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);
        }

        // similarly an attempt at:
        // +---+
        // |   |
        // |   +===+
        // |   |
        // +---+
        //
        // will throw, which is good because it would need a fix-up of the following curve
        // across where the double bit is deleted
    }

    [Test]
    public void TestCoincidentCurves()
    {
        //// Including where only parts coincide
        //{
        //    // these make a set of boxes as follows:
        //    //
        //    //             +----------+              +----------+
        //    //             |          |              |          |
        //    //    +--------+          |     +--------+          |
        //    //    |        |          |     |                   |
        //    //    |        +------+   |     |        +------+   |
        //    //    |        |      |   |     |        |      |   |
        //    //    |   +----+      |   |     |   +----+      |   |
        //    //    |   |    |      |   |     |   |           |   |
        //    //    |   |    +--+   |   |     |   |    +--+   |   |
        //    //    |   |    |  |   |   | ==> |   |    |  |   |   |
        //    //    |   |    +--+   |   |     |   |    +--+   |   |
        //    //    |   |    |      |   |     |   |           |   |
        //    //    |   +----+      |   |     |   +----+      |   |
        //    //    |        |      |   |     |        |      |   |
        //    //    |        +------+   |     |        +------+   |
        //    //    |        |          |     |                   |
        //    //    +--------+          |     +--------+          |
        //    //             |          |              |          |
        //    //             +----------+              +----------+
        //    //
        //    // (all +ve)
        //    LoopSet box5 = new LoopSet(
        //        Loop.MakePolygon(new List<Vector2> {
        //            new Vector2(0, 0),
        //            new Vector2(0, 10),
        //            new Vector2(10, 10),
        //            new Vector2(10, 0),
        //        }, RotationDirection.Forwards)
        //    );
        //    LoopSet box4 = new LoopSet(
        //        Loop.MakePolygon(new List<Vector2> {
        //            new Vector2(0, 1),
        //            new Vector2(0, 9),
        //            new Vector2(-8, 9),
        //            new Vector2(-8, 1),
        //        }, RotationDirection.Forwards)
        //    );
        //    LoopSet box3 = new LoopSet(
        //        Loop.MakePolygon(new List<Vector2> {
        //            new Vector2(0, 2),
        //            new Vector2(0, 8),
        //            new Vector2(6, 8),
        //            new Vector2(6, 2),
        //        }, RotationDirection.Reverse)
        //    );
        //    LoopSet box2 = new LoopSet(
        //        Loop.MakePolygon(new List<Vector2> {
        //            new Vector2(0, 3),
        //            new Vector2(0, 7),
        //            new Vector2(-4, 7),
        //            new Vector2(-4, 3),
        //        }, RotationDirection.Reverse)
        //    );
        //    LoopSet box1 = new LoopSet(
        //        Loop.MakePolygon(new List<Vector2> {
        //            new Vector2(0, 4),
        //            new Vector2(0, 6),
        //            new Vector2(2, 6),
        //            new Vector2(2, 4),
        //        }, RotationDirection.Forwards)
        //    );

        //    // with the old implementation, one out of forwards and backwards on this should fail
        //    // (because the order of presenting the two curves will make it look like a zero-sized
        //    // +ve poly or a zero-sized -ve poly...)

        //    // box4 and box5 presented together should just merge
        //    LoopSet merged = m_intersector.Union(new LoopSet(), box5.Concatenate(box4), 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

        //    Assert.AreEqual(1, merged.Count);
        //    Assert.AreEqual()

        //    merged = m_intersector.Union(new LoopSet(), ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

        //    Assert.AreEqual(0, merged.Count);
        //}

        {
            // two opposite circles should disappear
            LoopSet ls1 = new LoopSet(
                new List<Loop> {
                    new Loop("", new CircleCurve(new Vector2(), 1, RotationDirection.Forwards)),
                    new Loop("", new CircleCurve(new Vector2(), 1, RotationDirection.Reverse)),
                }
            );

            LoopSet ls1r = ls1.Reversed();

            // with the old implementation, one out of forwards and backwards on this should fail
            // (because the order of presenting the two curves will make it look like a zero-sized
            // +ve poly or a zero-sized -ve poly...)
            LoopSet merged = m_intersector.Union(new LoopSet(), ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);

            merged = m_intersector.Union(new LoopSet(), ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);
        }

        {
            // rects that take out parts of each other should work
            LoopSet ls1 = new LoopSet(Loop.MakeRect(0, 0, 20, 20));
            LoopSet ls2 = new LoopSet(Loop.MakeRect(5, 0, 15, 20).Reversed());

            LoopSet merged = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(2, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            merged = m_intersector.Union(ls2, ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(2, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            merged = m_intersector.Union(ls2.Reversed(), ls1.Reversed(), 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            // in this case the outer -ve box should disappear, taking the inner box with it
            Assert.AreEqual(0, merged.Count);

            merged = m_intersector.Union(ls1.Reversed(), ls2.Reversed(), 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(0, merged.Count);
        }

        {
            // rects that abut should work
            LoopSet ls1 = new LoopSet(Loop.MakeRect(0, 0, 20, 20));
            LoopSet ls2 = new LoopSet(Loop.MakeRect(5, -10, 15, 0).Reversed());

            // -ve touching rect should just dissappear
            LoopSet merged = m_intersector.Union(ls1, ls2, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            merged = m_intersector.Union(ls2, ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            // +ve touching rect should get merged in
            merged = m_intersector.Union(ls1, ls2.Reversed(), 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(new Box2(0, -10, 20, 20), merged.GetBounds());

            merged = m_intersector.Union(ls2.Reversed(), ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(new Box2(0, -10, 20, 20), merged.GetBounds());
        }
    }

    private void FixCrossings(IList<Intersector.Interval> list)
    {
        int crossings = 0;

        for (int i = 0; i < list.Count; i++)
        {
            // relying on the fact I set all the dot-products to +/- 1
            Intersector.Interval interval = list[i];

            crossings += (int)interval.DotProduct;
            var interval2 = new Intersector.Interval(interval.Curve, crossings, interval.DotProduct, interval.Distance);

            list[i] = interval2;
        }
    }

    // crossing number should only ever vary by 1, irrespective of what we delete
    private void CheckCrossings(IList<Intersector.Interval> list)
    {
        int crossing = list[0].CrossingNumber;

        for (int i = 1; i < list.Count; i++)
        {
            int here_crossings = list[i].CrossingNumber;
            Assert.AreEqual(1, Math.Abs(crossing - here_crossings));
            crossing = here_crossings;
        }

        // and we should always get back to zero
        // nope, not for arbitrary test cases
        // Assert.AreEqual(0, crossing);
    }

    readonly Curve l = LineCurve.MakeFromPoints(0, 0, 1, 0);
    readonly Curve lr = LineCurve.MakeFromPoints(1, 0, 0, 0);

    private void AddIntervals(string code, float dist, List<Intersector.Interval> list)
    {
        for (int i = 0; i < code.Length; i++)
        {
            if (code[i] == 'p')
            {
                list.Add(new Intersector.Interval(l, 0, 1, dist));
            }
            else
            {
                list.Add(new Intersector.Interval(lr, 0, -1, dist));
            }
        }

        FixCrossings(list);
    }

    [Test]
    public void TestEliminateCancellingLines()
    {
        {
            // whatever we add separated in distance doesn't get eliminated
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("p", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);

            AddIntervals("r", 1, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(2, list.Count);
            CheckCrossings(list);

            AddIntervals("p", 2, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(3, list.Count);
            CheckCrossings(list);
        }

        {
            // multiple lines with the same orientation should not cancel
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("pp", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(2, list.Count);
            CheckCrossings(list);

            AddIntervals("p", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(3, list.Count);
            CheckCrossings(list);
        }

        {
            // two opposite lines at the same point should cancel
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("pr", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(0, list.Count);
        }

        {
            // multiple pairs of opposite lines should cancel
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("prprprpr", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(0, list.Count);
        }

        {
            // order should not matter
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("pppprrrr", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(0, list.Count);

            AddIntervals("rrrrpppp", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(0, list.Count);

            AddIntervals("pprrprrp", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(0, list.Count);
        }

        {
            // any excess +ve or -ve lines should persist
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("ppprr", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);

            list.Clear();

            AddIntervals("rrrrppppr", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);

            list.Clear();

            AddIntervals("prpprrprp", 0, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);
        }

        {
            // multiple separate groups should behave the same
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("prpr", 0, list);
            AddIntervals("prpr", 1, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(0, list.Count);

            AddIntervals("prppr", 0, list);
            AddIntervals("prpr", 1, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0, list[0].Distance);
            CheckCrossings(list);

            list.Clear();

            AddIntervals("prpr", 0, list);
            AddIntervals("pprpr", 1, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list[0].Distance);
            CheckCrossings(list);
        }

        {
            // if there is lots of counting crossings up and down, it should remain stepwise after any cancellings
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("prprpp", 0, list);
            AddIntervals("prpr", 1, list);
            AddIntervals("prrpr", 2, list);
            AddIntervals("r", 3, list);

            m_intersector.EliminateCancellingLines(list, null, 1e-4f, null);

            Assert.AreEqual(4, list.Count);
            CheckCrossings(list);
        }
    }

    [Test]
    public void TestClusterJoints()
    {
        {
            // widely separated points should come through unchanged
            HashSet<Vector2> set = new HashSet<Vector2>
            {
                new Vector2(0, 0)
            };

            var ret = m_intersector.ClusterJoints(set, 1.0f);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(set, ret);

            // --

            set.Add(new Vector2(10, 0));

            ret = m_intersector.ClusterJoints(set, 1.0f);

            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(set, ret);

            set.Add(new Vector2(10, 10));

            ret = m_intersector.ClusterJoints(set, 1.0f);

            Assert.AreEqual(3, ret.Count);
            Assert.AreEqual(set, ret);
        }

        {
            // clusters should decay into their centroids
            HashSet<Vector2> set = new HashSet<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),

                new Vector2(10, 0),
                new Vector2(11, 0),
                new Vector2(11, 1),
                new Vector2(10, 1),

                new Vector2(0, 10),
                new Vector2(1, 10),
                new Vector2(1, 11),
                new Vector2(0, 11),
            };

            var ret = m_intersector.ClusterJoints(set, 2.0f);

            Assert.AreEqual(3, ret.Count);
            Assert.IsTrue(ret.Contains(new Vector2(0.5f, 0.5f)));
            Assert.IsTrue(ret.Contains(new Vector2(10.5f, 0.5f)));
            Assert.IsTrue(ret.Contains(new Vector2(0.5f, 10.5f)));
        }

        {
            // order is irrelevant
            HashSet<Vector2> set = new HashSet<Vector2>
            {
                new Vector2(1, 1),
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 10),
                new Vector2(0, 11),
                new Vector2(11, 0),
                new Vector2(11, 1),
                new Vector2(10, 1),
                new Vector2(0, 10),
                new Vector2(1, 11),
            };

            var ret = m_intersector.ClusterJoints(set, 2.0f);

            Assert.AreEqual(3, ret.Count);
            Assert.IsTrue(ret.Contains(new Vector2(0.5f, 0.5f)));
            Assert.IsTrue(ret.Contains(new Vector2(10.5f, 0.5f)));
            Assert.IsTrue(ret.Contains(new Vector2(0.5f, 10.5f)));
        }
    }

    [Test]
    public void TestSplitCurvesAtIntersections_SpliceMap()
    {
        {
            // circles miss
            Curve cc1 = new CircleCurve(new Vector2(), 1);
            Curve cc2 = new CircleCurve(new Vector2(3, 0), 1);

            List<Curve> curves1 = new List<Curve>
            {
                cc1
            };

            List<Curve> curves2 = new List<Curve>
            {
                cc2
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // circles meet at two points
            Curve cc1 = new CircleCurve(new Vector2(), 1);
            Curve cc2 = new CircleCurve(new Vector2(1, 0), 1);

            List<Curve> curves1 = new List<Curve>
            {
                cc1
            };

            List<Curve> curves2 = new List<Curve>
            {
                cc2
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // circles meet at one point
            Curve cc1 = new CircleCurve(new Vector2(), 1);
            Curve cc2 = new CircleCurve(new Vector2(2, 0), 1);

            List<Curve> curves1 = new List<Curve>
            {
                cc1
            };

            List<Curve> curves2 = new List<Curve>
            {
                cc2
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // circles meet at one point and it hits the join in one
            Curve cc1 = new CircleCurve(new Vector2(), 1);
            Curve cc2 = new CircleCurve(new Vector2(0, 2), 1);

            List<Curve> curves1 = new List<Curve>
            {
                cc1
            };

            List<Curve> curves2 = new List<Curve>
            {
                cc2
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // as above, but other way around
            Curve cc1 = new CircleCurve(new Vector2(), 1);
            Curve cc2 = new CircleCurve(new Vector2(0, 2), 1);

            List<Curve> curves1 = new List<Curve>
            {
                cc1
            };

            List<Curve> curves2 = new List<Curve>
            {
                cc2
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves2, curves1, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // circles meet at one point and it hits the join in both
            Curve cc1 = new CircleCurve(new Vector2(), 1);
            Curve cc2 = new CircleCurve(new Vector2(0, 2), 1, Mathf.PI, Mathf.PI * 3);

            List<Curve> curves1 = new List<Curve>
            {
                cc1
            };

            List<Curve> curves2 = new List<Curve>
            {
                cc2
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // all splits hit existing gaps
            // circles meet at one point and it hits the join in both
            Curve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            Curve cc2 = new CircleCurve(new Vector2(0, 2), 1, Mathf.PI, Mathf.PI * 2);

            List<Curve> curves1 = new List<Curve>
            {
                cc1, cc2
            };

            List<Curve> curves2 = Loop.MakeRect(0, -1, 2, 1).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // figure 8 shape with another line hitting the cross over:
            //
            // +---+
            // |   |
            // +---+---+
            //     |   |
            //     +---+
            //
            // lines not crossing in middle, case

            List<Curve> curves1 = new List<Curve>
            {
                new LineCurve(new Vector2(0, 0), new Vector2(1, 0), 1),
                new LineCurve(new Vector2(1, 0), new Vector2(0, -1), 1),
                new LineCurve(new Vector2(1, -1), new Vector2(-1, 0), 1),
                new LineCurve(new Vector2(0, -1), new Vector2(0, 1), 1),
                new LineCurve(new Vector2(0, 0), new Vector2(-1, 0), 1),
                new LineCurve(new Vector2(-1, 0), new Vector2(0, 1), 1),
                new LineCurve(new Vector2(-1, 1), new Vector2(1, 0), 1),
                new LineCurve(new Vector2(0, 1), new Vector2(0, -1), 1),
            };

            new Loop("", curves1);

            List<Curve> curves2 = new List<Curve>
            {
                new LineCurve(new Vector2(-3, -3), new Vector2(1, 1).normalized, Mathf.Sqrt(72)),
                new LineCurve(new Vector2(3, 3), new Vector2(1, -1).normalized, Mathf.Sqrt(72)),
                new LineCurve(new Vector2(9, -3), new Vector2(-1, -1).normalized, Mathf.Sqrt(72)),
                new LineCurve(new Vector2(3, -9), new Vector2(-1, 1).normalized, Mathf.Sqrt(72)),
            };

            new Loop("", curves2);

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtIntersections(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }
    }

    private void ValidateSpliceMap(Dictionary<Curve, Intersector.Splice> endSpliceMap,
        IList<Curve> allCurves)
    {
        int size = allCurves.Count;

        Assert.AreEqual(size, endSpliceMap.Count);

        Dictionary<Curve, int> forward_counts = new Dictionary<Curve, int>(
            new Intersector.ReferenceComparer<Curve>());
        Dictionary<Curve, int> backward_counts = new Dictionary<Curve, int>(
            new Intersector.ReferenceComparer<Curve>());

        foreach(var c in allCurves)
        {
            Assert.IsTrue(endSpliceMap.ContainsKey(c));
        }

        foreach (var c in endSpliceMap.Keys)
        {
            Assert.IsTrue(endSpliceMap[c].BackwardLinks.Contains(c));
        }

        foreach (var c in endSpliceMap.Values.SelectMany(x => x.ForwardLinks))
        {
            forward_counts[c] = 0;
        }

        foreach (var c in endSpliceMap.Values.SelectMany(x => x.BackwardLinks))
        {
            backward_counts[c] = 0;
        }

        Assert.AreEqual(size, forward_counts.Count);
        Assert.AreEqual(size, backward_counts.Count);

        var hfk = new HashSet<Curve>(forward_counts.Keys,
            new Intersector.ReferenceComparer<Curve>());
        var hbk = new HashSet<Curve>(backward_counts.Keys,
            new Intersector.ReferenceComparer<Curve>());

        Assert.IsTrue(hfk.SetEquals(hbk));

        // looking at _unique_ splices, each curve should enter and exit exactly one
        foreach (var splice in endSpliceMap.Values.Distinct())
        {
            Assert.AreEqual(splice.ForwardLinks.Count, splice.BackwardLinks.Count);

            foreach(var c in splice.ForwardLinks)
            {
                Assert.IsTrue(endSpliceMap.ContainsKey(c));

                forward_counts[c]++;
            }

            foreach (var c in splice.BackwardLinks)
            {
                Assert.IsTrue(endSpliceMap.ContainsKey(c));

                backward_counts[c]++;
            }
        }

        foreach(var c in forward_counts.Keys)
        {
            Assert.AreEqual(1, forward_counts[c]);
            Assert.AreEqual(1, backward_counts[c]);
        }
    }

    [Test]
    public void TestSplitCoincidentCurves()
    {
        {
            // two circles, breaking in the same place
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(1, curves1.Count);
            Assert.AreEqual(1, curves2.Count);
        }

        {
            // two circles, breaking different places
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 3)
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(2, curves1.Count);
            Assert.AreEqual(2, curves2.Count);

            var left1 = curves1.OrderBy(x => x.Pos(0.5f, false).x).First() as CircleCurve;
            var left2 = curves2.OrderBy(x => x.Pos(0.5f, false).x).First() as CircleCurve;
            var right1 = curves1.OrderBy(x => x.Pos(0.5f, false).x).Last() as CircleCurve;
            var right2 = curves2.OrderBy(x => x.Pos(0.5f, false).x).Last() as CircleCurve;

            Assert.IsTrue(left1.Equals(left2, 1e-5f));
            Assert.IsTrue(right1.Equals(right2, 1e-5f));
        }

        {
            // two circles, one in two pieces
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, 0, Mathf.PI),
                new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 2)
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(2, curves1.Count);
            Assert.AreEqual(2, curves2.Count);

            var left1 = curves1.OrderBy(x => x.Pos(0.5f, false).x).First() as CircleCurve;
            var left2 = curves2.OrderBy(x => x.Pos(0.5f, false).x).First() as CircleCurve;
            var right1 = curves1.OrderBy(x => x.Pos(0.5f, false).x).Last() as CircleCurve;
            var right2 = curves2.OrderBy(x => x.Pos(0.5f, false).x).Last() as CircleCurve;

            Assert.IsTrue(left1.Equals(left2, 1e-5f));
            Assert.IsTrue(right1.Equals(right2, 1e-5f));
        }

        for(int i = 0; i < 6; i++)
        {
            // two circles, one in three pieces
            // (requiring the other to be split twice for one segment of the first)
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, 0, Mathf.PI - 1),
                new CircleCurve(new Vector2(), 1, Mathf.PI - 1, Mathf.PI + 1),
                new CircleCurve(new Vector2(), 1, Mathf.PI + 1, Mathf.PI * 2),
            };

            // the order of presentation might matter here, so try all cyclic permutations
            curves2 = curves2.Skip(i % 3).Concat(curves2).Take(3).ToList();

            if (i >= 3)
            {
                var temp = curves1;
                curves1 = curves2;
                curves2 = temp;
            }

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(3, curves1.Count);
            Assert.AreEqual(3, curves2.Count);

            var left1 = curves1.OrderBy(x => x.Pos(0.5f, false).x).First() as CircleCurve;
            var left2 = curves2.OrderBy(x => x.Pos(0.5f, false).x).First() as CircleCurve;
            var mid1 = curves2.OrderBy(x => x.Pos(0.5f, false).x).Skip(1).First() as CircleCurve;
            var mid2 = curves2.OrderBy(x => x.Pos(0.5f, false).x).Skip(1).First() as CircleCurve;
            var right1 = curves1.OrderBy(x => x.Pos(0.5f, false).x).Last() as CircleCurve;
            var right2 = curves2.OrderBy(x => x.Pos(0.5f, false).x).Last() as CircleCurve;

            Assert.IsTrue(left1.Equals(left2, 1e-5f));
            Assert.IsTrue(mid1.Equals(mid2, 1e-5f));
            Assert.IsTrue(right1.Equals(right2, 1e-5f));
        }

        {
            // two rectangles, aligned along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0, 1, 1, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(4, curves1.Count);
            Assert.AreEqual(4, curves2.Count);
        }

        {
            // two rectangles, offset along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.5f, 1, 1.5f, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(5, curves1.Count);
            Assert.AreEqual(5, curves2.Count);
        }

        {
            // two rectangles, offset (other way) along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(-0.5f, 1, 0.5f, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(5, curves1.Count);
            Assert.AreEqual(5, curves2.Count);
        }

        {
            // two rectangles, touching on one edge, one inside in both directions
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 1, 0.75f, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(6, curves1.Count);
            Assert.AreEqual(4, curves2.Count);
        }

        {
            // two rectangles, one inside the other and smaller on one axis
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 0, 0.75f, 1).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            Assert.AreEqual(8, curves1.Count);
            Assert.AreEqual(4, curves2.Count);
        }
    }

    [Test]
    public void TestSplitCoincidentCurves_SpliceMap()
    {
        {
            // two circles, breaking different places
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 3)
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // two circles, one in two pieces
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, 0, Mathf.PI),
                new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 2)
            };

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        for (int i = 0; i < 6; i++)
        {
            // two circles, one in three pieces
            // (requiring the other to be split twice for one segment of the first)
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, 0, Mathf.PI - 1),
                new CircleCurve(new Vector2(), 1, Mathf.PI - 1, Mathf.PI + 1),
                new CircleCurve(new Vector2(), 1, Mathf.PI + 1, Mathf.PI * 2),
            };

            // the order of presentation might matter here, so try all cyclic permutations
            curves2 = curves2.Skip(i % 3).Concat(curves2).Take(3).ToList();

            if (i >= 3)
            {
                var temp = curves1;
                curves1 = curves2;
                curves2 = temp;
            }

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // two rectangles, aligned along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0, 1, 1, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // two rectangles, offset along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.5f, 1, 1.5f, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // two rectangles, offset (other way) along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(-0.5f, 1, 0.5f, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // two rectangles, touching on one edge, one inside in both directions
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 1, 0.75f, 2).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }

        {
            // two rectangles, one inside the other and smaller on one axis
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 0, 0.75f, 1).Curves.ToList();

            var endSpliceMap = Intersector.MakeEndSpliceMap();

            m_intersector.SetupInitialSplices(curves1, endSpliceMap);
            m_intersector.SetupInitialSplices(curves2, endSpliceMap);

            m_intersector.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f, endSpliceMap);

            ValidateSpliceMap(endSpliceMap, curves1.Concat(curves2).ToList());
        }
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
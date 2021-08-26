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
        var intr = new Intersector();

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

        intr.BuildAnnotationChains(curves);

        var ann_map = intr.GetAnnotationMap();

        foreach (Curve c in curves)
        {
            Assert.IsNotNull(ann_map[c]);
            Assert.IsNotNull(ann_map[c]);
            Assert.AreEqual(1, ann_map[c].LoopNumber);
        }
    }

    [Test]
    public void TestSplitCurvesAtIntersections_TwoCirclesTwoPoints()
    {
        var intr = new Intersector();

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

        intr.BuildAnnotationChains(curves1);
        intr.BuildAnnotationChains(curves2);

        intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

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
        var intr = new Intersector();

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

        intr.BuildAnnotationChains(curves1);
        intr.BuildAnnotationChains(curves2);

        intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

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
        var intr = new Intersector();

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

        intr.BuildAnnotationChains(curves1);
        intr.BuildAnnotationChains(curves2);

        intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

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
        var intr = new Intersector();

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

        intr.BuildAnnotationChains(curves1);
        intr.BuildAnnotationChains(curves2);

        intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

        Assert.AreEqual(1, curves1.Count);
        Assert.AreEqual(2, curves2.Count);
    }

    [Test]
    public void TestTryFindIntersections()
    {
        // one circle, expect 1, 0
        {
            var intr = new Intersector();

            CircleCurve cc = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(cc);

            HashSet<Vector2> curve_joints = new HashSet<Vector2>
            {
                cc.StartPos
            };

            var ret = intr.TryFindIntersections(
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
            var intr = new Intersector();

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
                  intr.TryFindIntersections(
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
            var intr = new Intersector();

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
                  intr.TryFindIntersections(
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
            var intr = new Intersector();

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
                  intr.TryFindCurveIntersections(
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
            var intr = new Intersector();

            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(new List<Curve>
                {
                    cc1
                }
            );

            LineCurve lc = new LineCurve(new Vector2(-10, 0), new Vector2(0, 1), 20);

            var ret =
                  intr.TryFindCurveIntersections(
                        lc,
                        all_curves);

            Assert.IsNull(ret);
        }

        // clip the circle, to simplify the analysis we disregard these, expect null
        {
            var intr = new Intersector();

            CircleCurve cc1 = new CircleCurve(new Vector2(), 5);

            HashSet<Curve> all_curves = Intersector.MakeAllCurvesSet(new List<Curve>
                {
                    cc1
                }
            );

            LineCurve lc = new LineCurve(new Vector2(-5, -5), new Vector2(0, 1), 20);

            var ret =
                  intr.TryFindCurveIntersections(
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
            var intr = new Intersector();

            Loop ls2 = new Loop();

            intr.Union(ls2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

            Assert.IsTrue(ret.Count == 0);
        }

        // something union nothing should equal something
        {
            var intr = new Intersector();

            Loop ls2 = new Loop();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));

            intr.SetInitialLoop(l1);
            intr.Union(ls2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(l1, ret[0]);
        }

        // nothing union something should equal something
        {
            var intr = new Intersector();

            Loop l2 = new Loop("", new CircleCurve(new Vector2(), 1));

            intr.Union(l2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(l2, ret[0]);
        }

        // union of two identical things should equal either one of them
        {
            var intr = new Intersector();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            Loop l2 = new Loop("", new CircleCurve(new Vector2(), 1));

            // paranoia
            Assert.AreEqual(l1, l2);

            intr.SetInitialLoop(l1);
            intr.Union(l2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(l1, ret[0]);
        }

        // union of two overlapping circles should be one two-part curve
        {
            var intr = new Intersector();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            Loop l2 = new Loop("", new CircleCurve(new Vector2(1, 0), 1));

            intr.SetInitialLoop(l1);
            intr.Union(l2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

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
            var intr = new Intersector();

            Loop l1a = new Loop("", new CircleCurve(new Vector2(), 1));
            Loop l1b = new Loop("", new CircleCurve(new Vector2(), 0.3f, RotationDirection.Reverse));
            intr.SetInitialLoop(l1a);
            intr.Union(l1b, 1e-5f, new ClRand(1));

            Loop l2a = new Loop("", new CircleCurve(new Vector2(1, 0), 1));
            Loop l2b = new Loop("", new CircleCurve(new Vector2(1, 0), 0.3f, RotationDirection.Reverse));
            Intersector ls2 = new Intersector(l2a);
            ls2.Union(l2b, 1e-5f, new ClRand(1));

            intr.Union(ls2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

            Assert.IsNotNull(ret);
            Assert.AreEqual(3, ret.Count);

            CheckLoop(ret[0], 2);
            CheckLoop(ret[1], 2);
            CheckLoop(ret[2], 2);
        }

        // osculating circles, outside each other
        {
            var intr = new Intersector();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));

            Loop l2 = new Loop("", new CircleCurve(new Vector2(2, 0), 1));

            intr.SetInitialLoop(l1);
            intr.Union(l2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

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
            var intr = new Intersector();

            LoopSet ls1 = new LoopSet();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));

            Loop l2 = new Loop("", new CircleCurve(new Vector2(2, 0), 1));

            intr.SetInitialLoop(l2);
            intr.Union(l1, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

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
            var intr = new Intersector();

            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));

            Loop l2 = new Loop("", new CircleCurve(new Vector2(0.5f, 0), 0.5f, RotationDirection.Reverse));

            intr.SetInitialLoop(l1);
            intr.Union(l2, 1e-5f, new ClRand(1));
            LoopSet ret = intr.Merged;

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
            HashSet<Curve> all_curves,
            HashSet<Curve> open,
            HashSet<Vector2> curve_joints,
            float diameter,
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
            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            Loop l2 = new Loop("", new CircleCurve(new Vector2(), 1));

            Intersector i = new Intersector(l1);

            i.Union(l2, 1e-5f, new ClRand(1));
            LoopSet ret = i.Merged;

            Assert.AreEqual(l1, ret[0]);
        }

        // if extractInternalCurves fails, we bail...
        {
            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            Loop l2 = new Loop("", new CircleCurve(new Vector2(1, 0), 1));

            Intersector i = new IntersectorDummy1();

            i.SetInitialLoop(l1);
            i.Union(l2, 1e-5f, new ClRand(1));
            LoopSet ret = i.Merged;

            Assert.AreEqual(0, ret.Count);
        }

        // if tryFindIntersections fails, we throw
        {
            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));

            Loop l2 = new Loop("", new CircleCurve(new Vector2(0.5f, 0), 1));

            Intersector i = new IntersectorDummy2();

            bool caught = false;

            try
            {
                i.SetInitialLoop(l1);
                i.Union(l2, 1e-5f, new ClRand(1));
            }
            catch (AnalysisFailedException)
            {
                caught = true;
            }

            Assert.IsTrue(caught);
        }

        // if tryFindIntersections fails (for a different reason), we throw
        {
            Loop l1 = new Loop("", new CircleCurve(new Vector2(), 1));
            Loop l2 = new Loop("", new CircleCurve(new Vector2(0.5f, 0), 1));

            Intersector i = new IntersectorDummy3();

            bool caught = false;

            try
            {
                i.SetInitialLoop(l1);
                i.Union(l2, 1e-5f, new ClRand(1));
            }
            catch (AnalysisFailedException)
            {
                caught = true;
            }

            Assert.IsTrue(caught);
        }

        {
            var intr = new Intersector();

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
                pcentre, p1tr, p2tr,
                pcentre, p1br, p2br,
                pcentre, p1bl, p2bl,
            }, RotationDirection.Forwards);

            intr.Union(l1, 1e-4f, new ClRand(1));
        }
    }

    [Test]
    public void TestLineClearsPoints()
    {
        LineCurve lc1 = new LineCurve(new Vector2(), new Vector2(1, 0), 10);
        LineCurve lc2 = new LineCurve(new Vector2(), new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), 10);

        {
            var intr = new Intersector();

            HashSet<Vector2> hs = new HashSet<Vector2>
            {
                new Vector2(1, 1)
            };

            Assert.IsTrue(intr.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsFalse(intr.LineClearsPoints(lc2, hs, 1e-5f));
        }

        {
            var intr = new Intersector();

            HashSet<Vector2> hs = new HashSet<Vector2>
            {
                new Vector2(0, 0)
            };

            Assert.IsFalse(intr.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsFalse(intr.LineClearsPoints(lc2, hs, 1e-5f));
        }

        {
            var intr = new Intersector();

            HashSet<Vector2> hs = new HashSet<Vector2>
            {
                new Vector2(2, 0)
            };

            Assert.IsFalse(intr.LineClearsPoints(lc1, hs, 1e-5f));
            Assert.IsTrue(intr.LineClearsPoints(lc2, hs, 1e-5f));
        }
    }

    [Test]
    public void TestRemoveTinyCurves()
    {
        {
            var intr = new Intersector();

            // discard a whole tiny circle
            var loop = new List<Curve>
            {
                new CircleCurve(new Vector2(), 0.1f)
            };

            Assert.IsNull(intr.RemoveTinyCurves(loop, 1));

            var ret = intr.RemoveTinyCurves(loop, 0.1f);

            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(loop[0], ret[0]);
        }

        {
            var intr = new Intersector();

            // discard a whole tiny rect
            var loop = Loop.MakeRect(0, 0, 0.1f, 0.1f).Curves.ToList();

            Assert.IsNull(intr.RemoveTinyCurves(loop, 1));

            var ret = intr.RemoveTinyCurves(loop, 0.1f);

            Assert.IsNotNull(ret);
            Assert.AreEqual(4, ret.Count);
            Assert.IsTrue(loop.SequenceEqual(ret));
        }

        {
            var intr = new Intersector();

            Vector2 pos = new Vector2();

            var loop = new List<Curve>()
            {
                BuildPolyStepwise(ref pos, new Vector2(0, 1)),
                BuildPolyStepwise(ref pos, new Vector2(1, 0)),
                BuildPolyStepwise(ref pos, new Vector2(0, -1)),
            };

            var ps = new Vector2(1, 0);
            var dir = new Vector2(-1, 0);

            for (int i = 0; i < 100; i++)
            {
                loop.Add(BuildPolyStepwise(ref pos, new Vector2(-0.01f, 0)));
            }

            new Loop("", loop);

            var ret = intr.RemoveTinyCurves(loop, 0.1f);

            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count < 103);
            // all we can do is assert that we increased the minimum
            Assert.IsTrue(ret.Select(x => x.Length).Min() > 0.01f);
        }

        {
            var intr = new Intersector();

            Vector2 pos = new Vector2();

            var loop = new List<Curve>()
            {
                BuildPolyStepwise(ref pos, new Vector2(0, 0.01f)),
                BuildPolyStepwise(ref pos, new Vector2(0, 1)),
                BuildPolyStepwise(ref pos, new Vector2(0, 0.01f)),
                BuildPolyStepwise(ref pos, new Vector2(0.01f, 0)),
                BuildPolyStepwise(ref pos, new Vector2(1, 0)),
                BuildPolyStepwise(ref pos, new Vector2(0.01f, 0)),
                BuildPolyStepwise(ref pos, new Vector2(0, -0.01f)),
                BuildPolyStepwise(ref pos, new Vector2(0, -1)),
                BuildPolyStepwise(ref pos, new Vector2(0, -0.01f)),
                BuildPolyStepwise(ref pos, new Vector2(-0.01f, 0)),
                BuildPolyStepwise(ref pos, new Vector2(-1, 0)),
                BuildPolyStepwise(ref pos, new Vector2(-0.01f, 0)),
            };

            new Loop("", loop);

            var ret = intr.RemoveTinyCurves(loop, 0.1f);

            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count == 4);
            // all we should have arrived very close to a unit square
            Assert.IsTrue(ret.Select(x => x.Length).Min() > 0.9f);
        }

        {
            var intr = new Intersector();

            // polygonize a circle built from many small bits (in real usage, curve merging would
            // join the circle parts if they were from identical circles...)

            var loop = new List<Curve>();

            for (int i = 0; i < 100; i++)
            {
                loop.Add(new CircleCurve(new Vector2(0, 0), 1, 2 * Mathf.PI * i / 100, 2 * Mathf.PI * (i + 1) / 100));
            }

            new Loop("", loop);

            var ret = intr.RemoveTinyCurves(loop, 0.1f);

            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count < 100);
            // all we can do is assert that we increased the minimum
            Assert.IsTrue(ret.Select(x => x.Length).Min() > 2 * Mathf.PI / 100);
        }
    }

    private Curve BuildPolyStepwise(ref Vector2 curr_pos, Vector2 step)
    {
        var next_pos = curr_pos + step;

        Curve ret = LineCurve.MakeFromPoints(curr_pos, next_pos);

        curr_pos = next_pos;

        return ret;
    }

    // this works but quite erratically,
    // generally it will fail in Intersector.ExtractLoop where it asserts:
    //                 Assertion.Assert(splice.Contains(start_ac)
    //                    || splice.Where(x => open.Contains(x)).Any());
    // because the allocation of splices to loop ends has messed up
    //
    // I think if I go over to assigning splices as:
    // 1) add them to all input curves
    // 2) as we split curves add new splices and adjust existing ones
    //
    // (it may be necessary to track forward and reverse splice connections to do this
    //  or if not to do it, to assert all is well)
    //
    // then the splices will stop messing up
    // but not my priority right now
    //
    // OK, WORKS MUCH BETTER NOW, but now fails when very small curves are generated
    //
    // NEEDS THAT REVIEW OF TOLERANCE HANDLING to try and come up with a consistent/stable scheme for
    // getting round numerical limitations...
    //[Test]
    //public void TestRandomUnions()
    //{
    //    const int NumTests = 1000;
    //    const int NumShapes = 5;

    //    for (int i = 0; i < NumTests; i++)
    //    {
    //        // let us jump straight to a given test
    //        ClRand test_rand = new ClRand(i);

    //        LoopSet merged = new LoopSet();

    //        for (int j = 0; j < NumShapes; j++)
    //        {
    //            LoopSet ls2 = RandShapeLoop(test_rand);

    //            // point here is to run all the Unions internal logic/asserts
    //            merged = m_intersector.Union(merged, ls2, 1e-5f, new ClRand(1));
    //            // any Union output should be good as input to the next stage
    //            m_intersector.Union(merged, new LoopSet(), 1e-5f, new ClRand(1));
    //        }
    //    }
    //}

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
            var intr = new Intersector();

            // clockwise is forwards is a positive polygon
            Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3, p4 }, RotationDirection.Forwards);

            intr.Union(l,
                1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet ret = intr.Merged;

            Assert.AreEqual(1, ret.Count);
            Assert.IsTrue(new HashSet<Curve>(l.Curves).SetEquals(ret[0].Curves));

            intr.Reset();

            intr.Union(l,
                1e-5f, new ClRand(1), Intersector.UnionType.WantNegative);
            ret = intr.Merged;

            Assert.AreEqual(0, ret.Count);
        }

        {
            var intr = new Intersector();

            // anti-clockwise is reverse is a negative polygon
            Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3, p4 }, RotationDirection.Reverse);

            intr.Union(l,
                1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet ret = intr.Merged;

            Assert.AreEqual(0, ret.Count);

            intr.Reset();

            intr.Union(l,
                1e-5f, new ClRand(1), Intersector.UnionType.WantNegative);
            ret = intr.Merged;

            Assert.AreEqual(1, ret.Count);
            Assert.IsTrue(new HashSet<Curve>(ret[0].Curves).SetEquals(l.Curves));
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

        Loop p1f = Loop.MakePolygon(p1, RotationDirection.Forwards);
        Loop p1r = Loop.MakePolygon(p1, RotationDirection.Reverse);
        Loop p2f = Loop.MakePolygon(p2, RotationDirection.Forwards);
        Loop p2r = Loop.MakePolygon(p2, RotationDirection.Reverse);
        Loop p3f = Loop.MakePolygon(p3, RotationDirection.Forwards);
        Loop p3r = Loop.MakePolygon(p3, RotationDirection.Reverse);
        Loop p4f = Loop.MakePolygon(p4, RotationDirection.Forwards);
        Loop p4r = Loop.MakePolygon(p4, RotationDirection.Reverse);

        var circ = new Loop("", new CircleCurve(new Vector2(0, 0), 1));

        {
            var intr = new Intersector(circ);

            intr.Union(p1f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(4, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(2, 1)), merged.GetBounds());
        }

        {
            var intr = new Intersector(circ);

            intr.Union(p1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(1, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            var intr = new Intersector(circ);

            intr.Union(p2f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(1, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            var intr = new Intersector(circ);

            intr.Union(p2r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(4, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-1, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            var intr = new Intersector(circ);

            intr.Union(p3f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(4, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-2, -1), new Vector2(1, 1)), merged.GetBounds());
        }

        {
            var intr = new Intersector(circ);

            intr.Union(p3r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

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
            var intr = new Intersector(circ);

            intr.Union(p4f, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(7, merged[0].Curves.Count);
            Assert.AreEqual(new Box2(new Vector2(-2, -1), new Vector2(2, 1)), merged.GetBounds());
        }

        {
            var intr = new Intersector(circ);

            intr.Union(p4r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

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
            var intr = new Intersector();

            // simple degenerate polygon should disappear
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);

            Loop ls1 = new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p1, p2),
                    LineCurve.MakeFromPoints(p2, p1),
                });

            Loop ls1r = new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p2, p1),
                    LineCurve.MakeFromPoints(p1, p2),
                });

            // with the old implementation, one out of forwards and backwards on this should fail
            // (because the order of presenting the two curves will make it look like a zero-sized
            // +ve poly or a zero-sized -ve poly...)
            intr.Union(ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);

            intr.Reset();

            intr.Union(ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);
        }

        {
            var intr = new Intersector();

            // slightly more complex degenerate polygon should disappear
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);
            Vector2 p3 = new Vector2(2, 0);
            Vector2 p4 = new Vector2(3, 0);

            // p1 -> p3 -> p2 -> p4 -> p1

            Loop ls1 = new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p1, p3),
                    LineCurve.MakeFromPoints(p3, p2),
                    LineCurve.MakeFromPoints(p2, p4),
                    LineCurve.MakeFromPoints(p4, p1),
                });

            Loop ls1r = ls1.Reversed();

            // with the old implementation, one out of forwards and backwards on this should fail
            // (because the order of presenting the two curves will make it look like a zero-sized
            // +ve poly or a zero-sized -ve poly...)
            intr.Union(ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);

            intr.Reset();

            intr.Union(ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);
        }

        {
            var intr = new Intersector();

            // degenerate U should disappear

            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);
            Vector2 p3 = new Vector2(1, 1);
            Vector2 p4 = new Vector2(0, 1);

            // p1 -> p3 -> p2 -> p4 -> p3 -> p2 -> p1

            Loop ls1 = new Loop("", new List<Curve>
                {
                    LineCurve.MakeFromPoints(p1, p2),
                    LineCurve.MakeFromPoints(p2, p3),
                    LineCurve.MakeFromPoints(p3, p4),
                    LineCurve.MakeFromPoints(p4, p3),
                    LineCurve.MakeFromPoints(p3, p2),
                    LineCurve.MakeFromPoints(p2, p1),
                });

            Loop ls1r = ls1.Reversed();

            intr.Union(ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);

            intr.Reset();

            intr.Union(ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

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
            var intr = new Intersector();

            // two opposite circles should disappear
            Loop ls1 = new Loop("",
                new List<Curve> {
                    new CircleCurve(new Vector2(), 1, RotationDirection.Forwards),
                    new CircleCurve(new Vector2(), 1, RotationDirection.Reverse),
                }
            );

            Loop ls1r = ls1.Reversed();

            // with the old implementation, one out of forwards and backwards on this should fail
            // (because the order of presenting the two curves will make it look like a zero-sized
            // +ve poly or a zero-sized -ve poly...)
            intr.Union(ls1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);

            intr.Reset();

            intr.Union(ls1r, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);
        }

        {
            var intr = new Intersector();

            // rects that take out parts of each other should work
            Loop l1 = Loop.MakeRect(0, 0, 20, 20);
            Loop l2 = Loop.MakeRect(5, 0, 15, 20).Reversed();

            intr.SetInitialLoop(l1);
            intr.Union(l2, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(2, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            intr.SetInitialLoop(l2);
            intr.Union(l1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            Assert.AreEqual(2, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            intr.SetInitialLoop(l2.Reversed());
            intr.Union(l1.Reversed(), 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            // in this case the outer -ve box should disappear, taking the inner box with it
            Assert.AreEqual(0, merged.Count);

            intr.Reset();

            intr.SetInitialLoop(l1.Reversed());
            intr.Union(l2.Reversed(), 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            Assert.AreEqual(0, merged.Count);
        }

        {
            var intr = new Intersector();

            // rects that abut should work
            Loop l1 = Loop.MakeRect(0, 0, 20, 20);
            Loop l2 = Loop.MakeRect(5, -10, 15, 0).Reversed();

            // -ve touching rect should just dissappear
            intr.SetInitialLoop(l1);
            intr.Union(l2, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            LoopSet merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            intr.SetInitialLoop(l2);
            intr.Union(l1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(new Box2(0, 0, 20, 20), merged.GetBounds());

            // +ve touching rect should get merged in
            intr.SetInitialLoop(l1);
            intr.Union(l2.Reversed(), 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(new Box2(0, -10, 20, 20), merged.GetBounds());

            intr.SetInitialLoop(l2.Reversed());
            intr.Union(l1, 1e-5f, new ClRand(1), Intersector.UnionType.WantPositive);
            merged = intr.Merged;

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
            var intr = new Intersector();

            // whatever we add separated in distance doesn't get eliminated
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("p", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);

            AddIntervals("r", 1, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(2, list.Count);
            CheckCrossings(list);

            AddIntervals("p", 2, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(3, list.Count);
            CheckCrossings(list);
        }

        {
            var intr = new Intersector();

            // multiple lines with the same orientation should not cancel
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("pp", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(2, list.Count);
            CheckCrossings(list);

            AddIntervals("p", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(3, list.Count);
            CheckCrossings(list);
        }

        {
            var intr = new Intersector();

            // two opposite lines at the same point should cancel
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("pr", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(0, list.Count);
        }

        {
            var intr = new Intersector();

            // multiple pairs of opposite lines should cancel
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("prprprpr", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(0, list.Count);
        }

        {
            var intr = new Intersector();

            // order should not matter
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("pppprrrr", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(0, list.Count);

            AddIntervals("rrrrpppp", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(0, list.Count);

            AddIntervals("pprrprrp", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(0, list.Count);
        }

        {
            var intr = new Intersector();

            // any excess +ve or -ve lines should persist
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("ppprr", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);

            list.Clear();

            AddIntervals("rrrrppppr", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);

            list.Clear();

            AddIntervals("prpprrprp", 0, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(1, list.Count);
            CheckCrossings(list);
        }

        {
            var intr = new Intersector();

            // multiple separate groups should behave the same
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("prpr", 0, list);
            AddIntervals("prpr", 1, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(0, list.Count);

            AddIntervals("prppr", 0, list);
            AddIntervals("prpr", 1, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0, list[0].Distance);
            CheckCrossings(list);

            list.Clear();

            AddIntervals("prpr", 0, list);
            AddIntervals("pprpr", 1, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list[0].Distance);
            CheckCrossings(list);
        }

        {
            var intr = new Intersector();

            // if there is lots of counting crossings up and down, it should remain stepwise after any cancellings
            List<Intersector.Interval> list = new List<Intersector.Interval>();

            AddIntervals("prprpp", 0, list);
            AddIntervals("prpr", 1, list);
            AddIntervals("prrpr", 2, list);
            AddIntervals("r", 3, list);

            intr.EliminateCancellingLines(list, null, 1e-4f);

            Assert.AreEqual(4, list.Count);
            CheckCrossings(list);
        }
    }

    [Test]
    public void TestClusterJoints()
    {
        {
            var intr = new Intersector();

            // widely separated points should come through unchanged
            HashSet<Vector2> set = new HashSet<Vector2>
            {
                new Vector2(0, 0)
            };

            var ret = intr.ClusterJoints(set, 1.0f);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(set, ret);

            // --

            set.Add(new Vector2(10, 0));

            ret = intr.ClusterJoints(set, 1.0f);

            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(set, ret);

            set.Add(new Vector2(10, 10));

            ret = intr.ClusterJoints(set, 1.0f);

            Assert.AreEqual(3, ret.Count);
            Assert.AreEqual(set, ret);
        }

        {
            var intr = new Intersector();

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

            var ret = intr.ClusterJoints(set, 2.0f);

            Assert.AreEqual(3, ret.Count);
            Assert.IsTrue(ret.Contains(new Vector2(0.5f, 0.5f)));
            Assert.IsTrue(ret.Contains(new Vector2(10.5f, 0.5f)));
            Assert.IsTrue(ret.Contains(new Vector2(0.5f, 10.5f)));
        }

        {
            var intr = new Intersector();

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

            var ret = intr.ClusterJoints(set, 2.0f);

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
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves2, curves1, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

            // all splits hit existing gaps
            // circles meet at one point and it hits the join in both
            Curve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            Curve cc2 = new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 2);

            List<Curve> curves1 = new List<Curve>
            {
                cc1, cc2
            };

            List<Curve> curves2 = Loop.MakeRect(0, -1, 2, 1).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtIntersections(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }
    }

    private void ValidateAnnotations(IReadOnlyDictionary<Curve, Intersector.AnnotatedCurve> ann_map,
        IList<Curve> allCurves)
    {
        int size = allCurves.Count;

        Assert.AreEqual(size, ann_map.Count);

        Dictionary<Curve, int> forward_counts = new Dictionary<Curve, int>(
            new Intersector.ReferenceComparer<Curve>());
        Dictionary<Curve, int> backward_counts = new Dictionary<Curve, int>(
            new Intersector.ReferenceComparer<Curve>());

        foreach(var c in allCurves)
        {
            Assert.IsTrue(ann_map.ContainsKey(c));
        }

        foreach (var c in ann_map.Keys)
        {
            Assert.IsTrue(ann_map[c].ForwardSplice.BackwardLinks.Contains(c));
            Assert.IsTrue(ann_map[c].BackwardSplice.ForwardLinks.Contains(c));
        }

        foreach (var c in ann_map.Values.SelectMany(x => x.ForwardSplice.ForwardLinks))
        {
            forward_counts[c] = 0;
        }

        foreach (var c in ann_map.Values.SelectMany(x => x.ForwardSplice.BackwardLinks))
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
        foreach (var splice in ann_map.Values.Select(x => x.ForwardSplice).Distinct())
        {
            Assert.AreEqual(splice.ForwardLinks.Count, splice.BackwardLinks.Count);

            foreach(var c in splice.ForwardLinks)
            {
                Assert.IsTrue(ann_map.ContainsKey(c));

                forward_counts[c]++;
            }

            foreach (var c in splice.BackwardLinks)
            {
                Assert.IsTrue(ann_map.ContainsKey(c));

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
            var intr = new Intersector();

            // two circles, breaking in the same place
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            Assert.AreEqual(1, curves1.Count);
            Assert.AreEqual(1, curves2.Count);
        }

        {
            var intr = new Intersector();

            // two circles, breaking different places
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 3)
            };

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

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
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

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
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

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
            var intr = new Intersector();

            // two rectangles, aligned along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0, 1, 1, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            Assert.AreEqual(4, curves1.Count);
            Assert.AreEqual(4, curves2.Count);
        }

        {
            var intr = new Intersector();

            // two rectangles, offset along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.5f, 1, 1.5f, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            Assert.AreEqual(5, curves1.Count);
            Assert.AreEqual(5, curves2.Count);
        }

        {
            var intr = new Intersector();

            // two rectangles, offset (other way) along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(-0.5f, 1, 0.5f, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            Assert.AreEqual(5, curves1.Count);
            Assert.AreEqual(5, curves2.Count);
        }

        {
            var intr = new Intersector();

            // two rectangles, touching on one edge, one inside in both directions
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 1, 0.75f, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            Assert.AreEqual(6, curves1.Count);
            Assert.AreEqual(4, curves2.Count);
        }

        {
            var intr = new Intersector();

            // two rectangles, one inside the other and smaller on one axis
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 0, 0.75f, 1).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            Assert.AreEqual(8, curves1.Count);
            Assert.AreEqual(4, curves2.Count);
        }
    }

    [Test]
    public void TestSplitCoincidentCurves_SpliceMap()
    {
        {
            var intr = new Intersector();

            // two circles, breaking different places
            List<Curve> curves1 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1)
            };

            List<Curve> curves2 = new List<Curve>
            {
                new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 3)
            };

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        for (int i = 0; i < 6; i++)
        {
            var intr = new Intersector();

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

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

            // two rectangles, aligned along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0, 1, 1, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

            // two rectangles, offset along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.5f, 1, 1.5f, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

            // two rectangles, offset (other way) along one edge
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(-0.5f, 1, 0.5f, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

            // two rectangles, touching on one edge, one inside in both directions
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 1, 0.75f, 2).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }

        {
            var intr = new Intersector();

            // two rectangles, one inside the other and smaller on one axis
            List<Curve> curves1 = Loop.MakeRect(0, 0, 1, 1).Curves.ToList();

            List<Curve> curves2 = Loop.MakeRect(0.25f, 0, 0.75f, 1).Curves.ToList();

            intr.BuildAnnotationChains(curves1);
            intr.BuildAnnotationChains(curves2);

            intr.SplitCurvesAtCoincidences(curves1, curves2, 1e-5f);

            ValidateAnnotations(intr.GetAnnotationMap(), curves1.Concat(curves2).ToList());
        }
    }

    [Test]
    public void TestCut()
    {
        // +-------+
        // |       | <========== R2
        // |   +---+---+
        // |   |   |   | <====== R1
        // +---+---+---+---+
        // |   |   |   |   |
        // |   +---+---+   | <== R4
        // |       |       |
        // +-------+-------+
        //    ^
        //     ================= R3
        //
        // + C is a circle inscribing R1

        var c = new Loop("", new CircleCurve(new Vector2(), 1));
        var r1 = Loop.MakeRect(-1, -1, 1, 1);
        var r2 = Loop.MakeRect(-2, 0, 0, 2);
        var r3 = Loop.MakeRect(-2, -2, 0, 0);
        var r4 = Loop.MakeRect(0, -2, 2, 0);

        {
            // circle cut by square that just contains it is nothing
            var intr = new Intersector(c);

            intr.Cut(r1, 1e-5f, new ClRand(1), "");

            Assert.AreEqual(0, intr.Merged.Count);
        }

        {
            // rect cut by itself is nothing
            var intr = new Intersector(r2);

            intr.Cut(r2, 1e-5f, new ClRand(1), "");

            Assert.AreEqual(0, intr.Merged.Count);
        }

        {
            // rect cut by just touching rects is unchanged
            var intr = new Intersector(r3);

            intr.Cut(r2, 1e-5f, new ClRand(1), "");

            Assert.AreEqual(1, intr.Merged.Count);
            Assert.AreEqual(r3, intr.Merged[0]);

            intr.Cut(r4, 1e-5f, new ClRand(1), "");

            Assert.AreEqual(1, intr.Merged.Count);
            Assert.AreEqual(r3, intr.Merged[0]);

            var i_comb = new Intersector(r3);
            i_comb.Union(r4, 1e-5f, new ClRand(0), "");

            intr.Cut(i_comb, 1e-5f, new ClRand(1), "");
        }

        {
            // three rects cut by circle
            var intr = new Intersector(r3);
            intr.Union(r2, 1e-5f, new ClRand(1));
            intr.Union(r4, 1e-5f, new ClRand(1));

            intr.Cut(c, 1e-5f, new ClRand(1), "");
            var m1 = intr.Merged;

            Assert.AreEqual(1, m1.Count);
            Assert.AreEqual(7, m1[0].NumCurves);
            Assert.AreEqual(new Box2(-2, -2, 2, 2), m1.GetBounds());
        }

        {
            // three rects cut by central rect
            var intr = new Intersector(r3);
            intr.Union(r2, 1e-5f, new ClRand(1));
            intr.Union(r4, 1e-5f, new ClRand(1));

            intr.Cut(r1, 1e-5f, new ClRand(1), "");
            var m1 = intr.Merged;

            Assert.AreEqual(1, m1.Count);
            Assert.AreEqual(10, m1[0].NumCurves);
            Assert.AreEqual(new Box2(-2, -2, 2, 2), m1.GetBounds());
        }

        {
            // central rect cut by three rects
            var intr = new Intersector(r1);
            intr.Cut(r2, 1e-5f, new ClRand(1));
            intr.Cut(r3, 1e-5f, new ClRand(1));
            intr.Cut(r4, 1e-5f, new ClRand(1));
            var m1 = intr.Merged;

            Assert.AreEqual(1, m1.Count);
            Assert.AreEqual(4, m1[0].NumCurves);
            Assert.AreEqual(new Box2(0, 0, 1, 1), m1.GetBounds());
        }

        {
            // circle cut by three rects
            var intr = new Intersector(c);
            intr.Cut(r2, 1e-5f, new ClRand(1));
            intr.Cut(r3, 1e-5f, new ClRand(1));
            intr.Cut(r4, 1e-5f, new ClRand(1));
            var m1 = intr.Merged;

            Assert.AreEqual(1, m1.Count);
            Assert.AreEqual(3, m1[0].NumCurves);
            Assert.AreEqual(new Box2(0, 0, 1, 1), m1.GetBounds());
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
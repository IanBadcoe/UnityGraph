using Assets.Generation.GeomRep;
using Assets.Extensions;
using Assets.Generation.U;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircleCurveTest
{
    [Test]
    public void TestCtor()
    {
        Assert.Throws<ArgumentException>(() => new CircleCurve(new Vector2(), -1));
        Assert.Throws<ArgumentException>(() => new CircleCurve(new Vector2(), 1, 0, 3 * Mathf.PI));
    }

    [Test]
    public void TestPos()
    {
        CircleCurve cc = new CircleCurve(new Vector2(), 1);

        {
            Vector2 p = cc.Pos(0, false);

            Assert.AreEqual(0, p.x, 1e-6);
            Assert.AreEqual(1, p.y, 1e-6);
        }

        {
            Vector2 p = cc.Pos(0.25f, false);

            Assert.AreEqual(1, p.x, 1e-6);
            Assert.AreEqual(0, p.y, 1e-6);
        }
    }

    [Test]
    public void TestHashCode()
    {
        CircleCurve cc1 = new CircleCurve(new Vector2(), 1);
        CircleCurve cc1b = new CircleCurve(new Vector2(), 1);
        CircleCurve cc2 = new CircleCurve(new Vector2(), 2);
        CircleCurve cc3 = new CircleCurve(new Vector2(1, 0), 1);

        Assert.AreNotEqual(cc1.GetHashCode(), cc2.GetHashCode());
        Assert.AreNotEqual(cc1.GetHashCode(), cc3.GetHashCode());
        Assert.AreNotEqual(cc2.GetHashCode(), cc3.GetHashCode());

        Assert.AreEqual(cc1b.GetHashCode(), cc1.GetHashCode());
    }

    [Test]
    public void TestEquals()
    {
        CircleCurve cc1 = new CircleCurve(new Vector2(), 1);
        CircleCurve cc1b = new CircleCurve(new Vector2(), 1);
        CircleCurve cc2 = new CircleCurve(new Vector2(), 2);
        CircleCurve cc3 = new CircleCurve(new Vector2(1, 0), 1);

        Assert.IsTrue(cc1.Equals(cc1b));
        Assert.IsFalse(cc1.Equals(cc2));
        Assert.IsFalse(cc1.Equals(cc3));
        Assert.IsFalse(cc2.Equals(cc3));

        Assert.IsFalse(cc1.Equals(1));
    }

    [Test]
    public void TestFindParamForPoint()
    {
        CircleCurve cc = new CircleCurve(new Vector2(), 1);
        CircleCurve ccr = new CircleCurve(new Vector2(), 1, RotationDirection.Reverse);

        {
            float? p = cc.FindParamForPoint(new Vector2(0, 1));

            Assert.AreEqual(0, p, 1e-6);
        }

        {
            float? p = cc.FindParamForPoint(new Vector2(1, 0));

            Assert.AreEqual(0.25f, p, 1e-6);
        }

        {
            float? p = ccr.FindParamForPoint(new Vector2(0, 1));

            // we're exacly on the join, so zero or one are good
            Assert.IsTrue(Math.Min(Math.Abs(p.Value - 1), Math.Abs(p.Value)) < 1e-6f);
        }

        {
            float? p = ccr.FindParamForPoint(new Vector2(1, 0));

            Assert.AreEqual(0.75f, p, 1e-6);
        }

        // we allow the point to be off the curve
        {
            float? p = cc.FindParamForPoint(new Vector2(0, 2));

            Assert.AreEqual(0, p, 1e-6);
        }

        // but not off the end of the params
        {
            CircleCurve cch = new CircleCurve(new Vector2(), 1, Mathf.PI / 2, 3 * Mathf.PI / 2);

            Assert.IsNull(cch.FindParamForPoint(new Vector2(0, 1)));
            Assert.AreEqual(0.5f, cch.FindParamForPoint(new Vector2(0, -1)), 1E-6f);
        }

        // but not off the end of the params (reversed)
        {
            CircleCurve cch = new CircleCurve(new Vector2(), 1, 3 * Mathf.PI / 2, Mathf.PI / 2);

            Assert.IsNull(cch.FindParamForPoint(new Vector2(0, 1)));
            Assert.AreEqual(0.5f, cch.FindParamForPoint(new Vector2(0, -1)), 1E-6f);
        }
    }

    [Test]
    public void TestCloneWithChangedExtents()
    {
        {
            CircleCurve cc = new CircleCurve(new Vector2(5, 6), 7);

            // trying to clone a curve with the opposite direction not supported
            // (could add if ever required)
            //
            // but might be confusing, because these are _params_ so forward and backward curves expect them in the
            // same order (0.75, 0.25) would give you a reverse of _whatever_ the input was
            Assert.Throws<ArgumentException>(() => cc.CloneWithChangedExtents(0.75f, 0.25f));
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(5, 6), 7);

            CircleCurve ccb = (CircleCurve)cc.CloneWithChangedExtents(0.25f, 0.75f);
            Assert.AreEqual(5, ccb.Position.x, 1e-6);
            Assert.AreEqual(6, ccb.Position.y, 1e-6);
            Assert.AreEqual(7, ccb.Radius, 1e-6);
            Assert.AreEqual(RotationDirection.Forwards, ccb.Rotation);
            Assert.AreEqual(Mathf.PI / 2, ccb.AngleRange.Start, 1e-6);
            Assert.AreEqual(3 * Mathf.PI / 2, ccb.AngleRange.End, 1e-6);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(5, 6), 7, RotationDirection.Reverse);

            CircleCurve ccb = (CircleCurve)cc.CloneWithChangedExtents(0.25f, 0.75f);
            Assert.AreEqual(5, ccb.Position.x, 1e-6);
            Assert.AreEqual(6, ccb.Position.y, 1e-6);
            Assert.AreEqual(7, ccb.Radius, 1e-6);
            Assert.AreEqual(RotationDirection.Reverse, ccb.Rotation);
            Assert.AreEqual(3 * Mathf.PI / 2, ccb.AngleRange.Start, 1e-6);
            Assert.AreEqual(Mathf.PI / 2, ccb.AngleRange.End, 1e-6);
        }
    }

    [Test]
    public void TestBoundingArea()
    {
        {
            CircleCurve cc = new CircleCurve(new Vector2(5, 6), 7);

            Box2 b = cc.BoundingArea;

            Assert.AreEqual(new Box2(new Vector2(-2, -1), new Vector2(12, 13)), b);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, 0, Mathf.PI / 2);

            Box2 b = cc.BoundingArea;

            Assert.AreEqual(new Box2(0, 0, 1, 1), b);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, Mathf.PI / 2, Mathf.PI);

            Box2 b = cc.BoundingArea;

            Assert.AreEqual(new Box2(0, -1, 1, 0), b);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, Mathf.PI, Mathf.PI * 2);

            Box2 b = cc.BoundingArea;

            Assert.AreEqual(new Box2(-1, -1, 0, 1), b);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, Mathf.PI / 4, 3 * Mathf.PI / 4);

            Box2 b = cc.BoundingArea;

            Assert.AreEqual(new Box2(1 / Mathf.Sqrt(2f), -1 / Mathf.Sqrt(2), 1, 1 / Mathf.Sqrt(2)), b);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, Mathf.PI / 4, 5 * Mathf.PI / 4);

            Box2 b = cc.BoundingArea;

            Assert.AreEqual(new Box2(-1 / Mathf.Sqrt(2f), -1, 1, 1 / Mathf.Sqrt(2)), b);
        }
    }

    [Test]
    public void TestTangent()
    {
        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1);

            Assert.IsTrue((new Vector2(1, 0) - cc.Tangent(0.0f)).magnitude < 1e-5f);
            Assert.IsTrue((new Vector2(0, -1) - cc.Tangent(0.25f)).magnitude < 1e-5f);
            Assert.IsTrue((new Vector2(-1, 0) - cc.Tangent(0.5f)).magnitude < 1e-5f);
            Assert.IsTrue((new Vector2(0, 1) - cc.Tangent(0.75f)).magnitude < 1e-5f);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, RotationDirection.Reverse);

            Assert.IsTrue((new Vector2(-1, 0) - cc.Tangent(0.0f)).magnitude < 1e-5f);
            Assert.IsTrue((new Vector2(0, -1) - cc.Tangent(0.25f)).magnitude < 1e-5f);
            Assert.IsTrue((new Vector2(1, 0) - cc.Tangent(0.5f)).magnitude < 1e-5f);
            Assert.IsTrue((new Vector2(0, 1) - cc.Tangent(0.75f)).magnitude < 1e-5f);
        }
    }

    [Test]
    public void TestMerge()
    {
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, Mathf.PI, 2 * Mathf.PI);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNotNull(cm);
            Assert.AreEqual(new Vector2(), cm.Position);
            Assert.AreEqual(1, cm.Radius, 0);
            Assert.AreEqual(RotationDirection.Forwards, cm.Rotation);
            Assert.AreEqual(0, cm.AngleRange.Start, 0);
            Assert.AreEqual(2 * Mathf.PI, cm.AngleRange.End, 0);

            // we don't care about presentation order
            cm = (CircleCurve)cc2.Merge(cc1);

            Assert.IsNotNull(cm);
            Assert.AreEqual(new Vector2(), cm.Position);
            Assert.AreEqual(1, cm.Radius, 0);
            Assert.AreEqual(RotationDirection.Forwards, cm.Rotation);
            // except in this case we think the break is in a different place
            Assert.AreEqual(Mathf.PI, cm.AngleRange.Start, 0);
            Assert.AreEqual(3 * Mathf.PI, cm.AngleRange.End, 0);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, Mathf.PI, 0);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, 2 * Mathf.PI, Mathf.PI);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNotNull(cm);
            Assert.AreEqual(new Vector2(), cm.Position);
            Assert.AreEqual(1, cm.Radius, 0);
            Assert.AreEqual(RotationDirection.Reverse, cm.Rotation);
            Assert.AreEqual(3 * Mathf.PI, cm.AngleRange.Start, 0);
            Assert.AreEqual(Mathf.PI, cm.AngleRange.End, 0);

            cm = (CircleCurve)cc2.Merge(cc1);

            Assert.IsNotNull(cm);
            Assert.AreEqual(new Vector2(), cm.Position);
            Assert.AreEqual(1, cm.Radius, 0);
            Assert.AreEqual(RotationDirection.Reverse, cm.Rotation);
            // except in this case we think the break is in a different place
            Assert.AreEqual(2 * Mathf.PI, cm.AngleRange.Start, 0);
            Assert.AreEqual(0, cm.AngleRange.End, 0);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, 2 * Mathf.PI, Mathf.PI);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNull(cm);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(1, 0), 1, Mathf.PI, 2 * Mathf.PI);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNull(cm);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 2, Mathf.PI, 2 * Mathf.PI);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNull(cm);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, 1);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, 1.5f, 2);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNull(cm);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc1);

            Assert.IsNull(cm);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            LineCurve cc2 = new LineCurve(new Vector2(), new Vector2(1, 0), 10);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNull(cm);
        }
    }

    [Test]
    public void TestLength()
    {
        CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
        CircleCurve cc2 = new CircleCurve(new Vector2(), 3, 0, 2 * Mathf.PI);

        Assert.AreEqual(Mathf.PI, cc1.Length, 1e-6);
        Assert.AreEqual(2 * Mathf.PI * 3, cc2.Length, 1e-6);
    }

    [Test]
    public void TestNormal()
    {
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI * 2);

            Assert.IsTrue(new Vector2(0, 1).Equals(cc1.Normal(0), 1e-5f));
            Assert.IsTrue(new Vector2(1, 0).Equals(cc1.Normal(0.25f), 1e-5f));
            Assert.IsTrue(new Vector2(0, -1).Equals(cc1.Normal(0.5f), 1e-5f));
            Assert.IsTrue(new Vector2(-1, 0).Equals(cc1.Normal(0.75f), 1e-5f));
            Assert.IsTrue(new Vector2(0, 1).Equals(cc1.Normal(1), 1e-5f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, Mathf.PI / 4, 5 * Mathf.PI / 4);

            Assert.IsTrue(new Vector2(1, 1).normalized.Equals(cc1.Normal(0), 1e-5f));
            Assert.IsTrue(new Vector2(1, 0).Equals(cc1.Normal(0.25f), 1e-5f));
            Assert.IsTrue(new Vector2(1, -1).normalized.Equals(cc1.Normal(0.5f), 1e-5f));
            Assert.IsTrue(new Vector2(0, -1).Equals(cc1.Normal(0.75f), 1e-5f));
            Assert.IsTrue(new Vector2(-1, -1).normalized.Equals(cc1.Normal(1), 1e-5f));
        }
    }

    [Test]
    public void TestWithinParams()
    {
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI / 2);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(1, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, 2 * Mathf.PI);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(1, 1e-6f));
            // ugh, these numbers are not really "within params"
            // but they are "interpretable as params" as with a full circle we can seamlessly run off either end
            Assert.IsTrue(cc1.WithinParams(2, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 3 * Mathf.PI / 2, 5 * Mathf.PI / 2);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(1, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, Mathf.PI / 2, 0);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(1, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 2 * Mathf.PI, 0);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(1, 1e-6f));
            // ugh, these numbers are not really "within params"
            // but they are "interpretable as params" as with a full circle we can seamlessly run off either end
            Assert.IsTrue(cc1.WithinParams(2, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 5 * Mathf.PI / 2, 3 * Mathf.PI / 2);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(1, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(-1, 1e-6f));
        }
    }

    [Test]
    public void TestIsCyclic()
    {
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, 2 * Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, 2 * Mathf.PI, 0);
            CircleCurve cc3 = new CircleCurve(new Vector2(), 1, Mathf.PI, 3 * Mathf.PI);
            CircleCurve cc4 = new CircleCurve(new Vector2(), 1, 3 * Mathf.PI, Mathf.PI);

            Assert.IsTrue(cc1.IsCyclic);
            Assert.IsTrue(cc2.IsCyclic);
            Assert.IsTrue(cc3.IsCyclic);
            Assert.IsTrue(cc4.IsCyclic);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0.1f, 2 * Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, 2 * Mathf.PI, 0.1f);
            CircleCurve cc3 = new CircleCurve(new Vector2(), 1, Mathf.PI + 0.1f, 3 * Mathf.PI);
            CircleCurve cc4 = new CircleCurve(new Vector2(), 1, 3 * Mathf.PI, Mathf.PI + 0.1f);

            Assert.IsFalse(cc1.IsCyclic);
            Assert.IsFalse(cc2.IsCyclic);
            Assert.IsFalse(cc3.IsCyclic);
            Assert.IsFalse(cc4.IsCyclic);
        }
    }

    [Test]
    public void TestSplitConcidentCurves()
    {
        Vector2 pos = new Vector2(0, 0);

        CircleCurve cc1 = new CircleCurve(pos, 1);
        CircleCurve cc1x1 = new CircleCurve(new Vector2(0.1f, 0), 1);
        CircleCurve cc1x2 = new CircleCurve(pos, 1.1f);

        Assert.IsNull(cc1.SplitCoincidentCurves(cc1x2, 1e-4f));
        Assert.IsNull(cc1.SplitCoincidentCurves(cc1x1, 1e-4f));

        CircleCurve cc2 = new CircleCurve(pos, 1, 0, 2);
        CircleCurve cc3 = new CircleCurve(pos, 1, 1, 3);
        CircleCurve cc4 = new CircleCurve(pos, 1, 0, 3);
        CircleCurve cc5 = new CircleCurve(pos, 1, 1, 2);

        CircleCurve cc6 = new CircleCurve(pos, 1, 1, 5);
        CircleCurve cc7 = new CircleCurve(pos, 1, -2, 2);

        // let's just check each partial circle against the full one
        {
            var curves = cc1.SplitCoincidentCurves(cc2, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1, new List<float> { 2 }, curves.Item1);
        }

        {
            var curves = cc1.SplitCoincidentCurves(cc3, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1, new List<float> { 1, 3 }, curves.Item1);
        }

        {
            var curves = cc1.SplitCoincidentCurves(cc4, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1, new List<float> { 3 }, curves.Item1);
        }

        {
            var curves = cc1.SplitCoincidentCurves(cc5, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1, new List<float> { 1, 2 }, curves.Item1);
        }

        // ranges overlapping at the end split both curves
        {
            var curves = cc2.SplitCoincidentCurves(cc3, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc2, new List<float> { 1 }, curves.Item1);
            CheckCurveSplit(cc3, new List<float> { 2 }, curves.Item2);
        }

        // and the same the other way around
        {
            var curves = cc3.SplitCoincidentCurves(cc2, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc3, new List<float> { 2 }, curves.Item1);
            CheckCurveSplit(cc2, new List<float> { 1 }, curves.Item2);
        }

        // ranges that are a subset split the other but are not split themselves
        {
            var curves = cc2.SplitCoincidentCurves(cc4, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc4, new List<float> { 2 }, curves.Item2);
        }

        // and the same the other way around
        {
            var curves = cc4.SplitCoincidentCurves(cc2, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc4, new List<float> { 2 }, curves.Item1);
        }

        // and the same (only two splits) if the subset is not at one end
        {
            var curves = cc5.SplitCoincidentCurves(cc4, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc4, new List<float> { 1, 2 }, curves.Item2);
        }

        // and the same the other way around
        {
            var curves = cc4.SplitCoincidentCurves(cc5, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc4, new List<float> { 1, 2 }, curves.Item1);
        }

        // ranges that sum to > 360 can generate two splits in both curves
        {
            var curves = cc6.SplitCoincidentCurves(cc7, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc6, new List<float> { 2, Mathf.PI * 2 - 2 }, curves.Item1);
            CheckCurveSplit(cc7, new List<float> { 5, Mathf.PI * 2 + 1 }, curves.Item2);
        }

        // ----
        // now repeat some of the above with one or more curves reversed
        // ----

        var cc1r = cc1.Reversed() as CircleCurve;
        var cc2r = cc2.Reversed() as CircleCurve;
        var cc3r = cc3.Reversed() as CircleCurve;
        var cc4r = cc4.Reversed() as CircleCurve;
        var cc5r = cc5.Reversed() as CircleCurve;

        var cc6r = cc6.Reversed() as CircleCurve;

        // let's just check each partial circle against the full one
        {
            var curves = cc1r.SplitCoincidentCurves(cc2, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1r, new List<float> { 2 }, curves.Item1);
        }

        // let's just check each partial circle against the full one
        {
            var curves = cc1.SplitCoincidentCurves(cc2r, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1, new List<float> { 2 }, curves.Item1);
        }

        {
            var curves = cc1r.SplitCoincidentCurves(cc3, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1r, new List<float> { 3, 1 }, curves.Item1);
        }

        {
            var curves = cc1.SplitCoincidentCurves(cc3r, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc1, new List<float> { 1, 3 }, curves.Item1);
        }

        // ranges overlapping at the end split both curves
        {
            var curves = cc2r.SplitCoincidentCurves(cc3, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc2r, new List<float> { 1 }, curves.Item1);
            CheckCurveSplit(cc3, new List<float> { 2 }, curves.Item2);
        }

        // and the same the other way around
        {
            var curves = cc3r.SplitCoincidentCurves(cc2, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc3r, new List<float> { 2 }, curves.Item1);
            CheckCurveSplit(cc2, new List<float> { 1 }, curves.Item2);
        }

        // ranges that are a subset split the other but are not split themselves
        {
            var curves = cc2r.SplitCoincidentCurves(cc4, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc4, new List<float> { 2 }, curves.Item2);
        }

        // and the same the other way around
        {
            var curves = cc4r.SplitCoincidentCurves(cc2, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc4r, new List<float> { 2 }, curves.Item1);
        }

        // and the same (only two splits) if the subset is not at one end
        {
            var curves = cc5r.SplitCoincidentCurves(cc4, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc4, new List<float> { 1, 2 }, curves.Item2);
        }

        // and the same the other way around
        {
            var curves = cc4r.SplitCoincidentCurves(cc5, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNull(curves.Item2);

            CheckCurveSplit(cc4r, new List<float> { 2, 1 }, curves.Item1);
        }

        // ranges that sum to > 360 can generate two splits in both curves
        {
            var curves = cc6r.SplitCoincidentCurves(cc7, 1e-4f);

            Assert.IsNotNull(curves);
            Assert.IsNotNull(curves.Item1);
            Assert.IsNotNull(curves.Item2);

            CheckCurveSplit(cc6r, new List<float> { Mathf.PI * 2 - 2, 2 }, curves.Item1);
            CheckCurveSplit(cc7, new List<float> { 5, Mathf.PI * 2 + 1 }, curves.Item2);
        }

        // midnight-crossing cases

        for (int i = 1; i < 10; i++)
        {
            float radians = -i * 0.5f;

            var cc1mc = RotatedAnticlockwise(cc1, radians);
            var cc2mc = RotatedAnticlockwise(cc2, radians);
            var cc3mc = RotatedAnticlockwise(cc3, radians);
            var cc4mc = RotatedAnticlockwise(cc4, radians);
            var cc5mc = RotatedAnticlockwise(cc5, radians);

            // let's just check each partial circle against the full one
            {
                var curves = cc1mc.SplitCoincidentCurves(cc2mc, 1e-4f);

                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNull(curves.Item2);

                CheckCurveSplit(cc1mc, new List<float> { cc2mc.AngleRange.End }, curves.Item1);
            }

            // ranges overlapping at the end split both curves
            {
                var curves = cc2mc.SplitCoincidentCurves(cc3mc, 1e-4f);

                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);

                CheckCurveSplit(cc2mc, new List<float> { cc3mc.AngleRange.Start }, curves.Item1);
                CheckCurveSplit(cc3mc, new List<float> { cc2mc.AngleRange.End }, curves.Item2);
            }

            {
                var curves = cc5mc.SplitCoincidentCurves(cc4mc, 1e-4f);

                Assert.IsNotNull(curves);
                Assert.IsNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);

                CheckCurveSplit(cc4mc, new List<float> { cc5mc.AngleRange.Start, cc5mc.AngleRange.End }, curves.Item2);
            }

            var cc1mcr = cc1mc.Reversed() as CircleCurve;
            var cc3mcr = cc3mc.Reversed() as CircleCurve;
            var cc5mcr = cc5mc.Reversed() as CircleCurve;

            // let's just check each partial circle against the full one
            {
                var curves = cc1mcr.SplitCoincidentCurves(cc2mc, 1e-4f);

                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNull(curves.Item2);

                CheckCurveSplit(cc1mcr, new List<float> { cc2mc.AngleRange.End }, curves.Item1);
            }

            // ranges overlapping at the end split both curves
            {
                var curves = cc2mc.SplitCoincidentCurves(cc3mcr, 1e-4f);

                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);

                CheckCurveSplit(cc2mc, new List<float> { cc3mcr.AngleRange.End }, curves.Item1);
                CheckCurveSplit(cc3mcr, new List<float> { cc2mc.AngleRange.End }, curves.Item2);
            }

            {
                var curves = cc5mcr.SplitCoincidentCurves(cc4mc, 1e-4f);

                Assert.IsNotNull(curves);
                Assert.IsNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);

                CheckCurveSplit(cc4mc, new List<float> { cc5mcr.AngleRange.End, cc5mcr.AngleRange.Start }, curves.Item2);
            }

        }
    }

    private CircleCurve RotatedAnticlockwise(CircleCurve cc, float radians)
    {
        return new CircleCurve(cc.Position, cc.Radius, cc.AngleRange.Start + radians, cc.AngleRange.End + radians);
    }

    private void CheckCurveSplit(CircleCurve input, List<float> splits, IList<Curve> in_curves)
    {
        var curves = in_curves.Cast<CircleCurve>().ToList();

        Assert.AreEqual(splits.Count + 1, curves.Count);

        Assert.IsTrue(Util.ClockAwareAngleCompare(input.AngleRange.Start, curves[0].AngleRange.Start, 1e-4f));
        Assert.IsTrue(Util.ClockAwareAngleCompare(input.AngleRange.End, curves.Last().AngleRange.End, 1e-4f));

        for (int i = 0; i < splits.Count; i++)
        {
            Assert.IsTrue(Util.ClockAwareAngleCompare(splits[i], curves[i].AngleRange.End, 1e-4f));
            Assert.IsTrue(Util.ClockAwareAngleCompare(splits[i], curves[i + 1].AngleRange.Start, 1e-4f));
        }

        foreach (var c in curves)
        {
            var lc = c as CircleCurve;
            Assert.IsNotNull(lc);
            Assert.IsTrue(input.SameSupercurve(lc, 1e-4f));
        }
    }
}

using Assets.Generation.GeomRep;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CircleCurveTest
{
    [Test]
    public void TestCtor()
    {
        {
            bool thrown = false;

            try
            {
                new CircleCurve(new Vector2(), -1);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, 0, Mathf.PI * 3);

            // automatically converted down to equivalent angle with only one turn
            Assert.AreEqual(Mathf.PI, cc.EndParam, 1e-6);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(), 1, Mathf.PI * 3, Mathf.PI * 4);

            // automatically converted down to equivalent angle with only one turn
            Assert.AreEqual(Mathf.PI, cc.StartParam, 1e-6);
            Assert.AreEqual(2 * Mathf.PI, cc.EndParam, 1e-6);
        }
    }

    [Test]
    public void TestComputePos()
    {
        CircleCurve cc = new CircleCurve(new Vector2(), 1);

        {
            Vector2 p = cc.Pos(0);

            Assert.AreEqual(0, p.x, 1e-6);
            Assert.AreEqual(1, p.y, 1e-6);
        }

        {
            Vector2 p = cc.Pos(Mathf.PI / 2);

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
        CircleCurve ccr = new CircleCurve(new Vector2(), 1, CircleCurve.RotationDirection.Reverse);

        {
            float? p = cc.FindParamForPoint(new Vector2(0, 1));

            Assert.AreEqual(0, p, 1e-6);
        }

        {
            float? p = cc.FindParamForPoint(new Vector2(1, 0));

            Assert.AreEqual(Mathf.PI / 2, p, 1e-6);
        }

        {
            float? p = ccr.FindParamForPoint(new Vector2(0, 1));

            Assert.AreEqual(0, p, 1e-6);
        }

        {
            float? p = ccr.FindParamForPoint(new Vector2(1, 0));

            Assert.AreEqual(3 * Mathf.PI / 2, p, 1e-6);
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
            Assert.AreEqual(Mathf.PI, cch.FindParamForPoint(new Vector2(0, -1)), 1E-6f);
        }
    }

    [Test]
    public void TestCloneWithChangedParams()
    {
        {
            CircleCurve cc = new CircleCurve(new Vector2(5, 6), 7);

            CircleCurve ccb = (CircleCurve)cc.CloneWithChangedParams(Mathf.PI / 2, 3 * Mathf.PI / 2);
            Assert.AreEqual(5, ccb.Position.x, 1e-6);
            Assert.AreEqual(6, ccb.Position.y, 1e-6);
            Assert.AreEqual(7, ccb.Radius, 1e-6);
            Assert.AreEqual(CircleCurve.RotationDirection.Forwards, ccb.Rotation);
            Assert.AreEqual(Mathf.PI / 2, ccb.StartParam, 1e-6);
            Assert.AreEqual(3 * Mathf.PI / 2, ccb.EndParam, 1e-6);
        }

        {
            CircleCurve cc = new CircleCurve(new Vector2(5, 6), 7, CircleCurve.RotationDirection.Reverse);

            CircleCurve ccb = (CircleCurve)cc.CloneWithChangedParams(Mathf.PI / 2, 3 * Mathf.PI / 2);
            Assert.AreEqual(5, ccb.Position.x, 1e-6);
            Assert.AreEqual(6, ccb.Position.y, 1e-6);
            Assert.AreEqual(7, ccb.Radius, 1e-6);
            Assert.AreEqual(CircleCurve.RotationDirection.Reverse, ccb.Rotation);
            Assert.AreEqual(Mathf.PI / 2, ccb.StartParam, 1e-6);
            Assert.AreEqual(3 * Mathf.PI / 2, ccb.EndParam, 1e-6);
        }
    }

    [Test]
    public void TestBoundingArea()
    {
        // for the moment we do not account for partial circles, so don't test that...
        CircleCurve cc = new CircleCurve(new Vector2(5, 6), 7);

        Box2 b = cc.BoundingArea;

        Assert.AreEqual(new Box2(new Vector2(-2, -1), new Vector2(12, 13)), b);
    }

    [Test]
    public void TestTangent()
    {
        CircleCurve cc = new CircleCurve(new Vector2(), 1);

        Assert.IsTrue((new Vector2(1, 0) - cc.Tangent(0.0f)).magnitude < 1e-5f);
        Assert.IsTrue((new Vector2(0, -1) - cc.Tangent(Mathf.PI / 2)).magnitude < 1e-5f);
        Assert.IsTrue((new Vector2(-1, 0) - cc.Tangent(Mathf.PI)).magnitude < 1e-5f);
        Assert.IsTrue((new Vector2(0, 1) - cc.Tangent(3 * Mathf.PI / 2)).magnitude < 1e-5f);
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
            Assert.AreEqual(CircleCurve.RotationDirection.Forwards, cm.Rotation);
            Assert.AreEqual(0, cm.StartParam, 0);
            Assert.AreEqual(2 * Mathf.PI, cm.EndParam, 0);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI, CircleCurve.RotationDirection.Reverse);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, Mathf.PI, 2 * Mathf.PI, CircleCurve.RotationDirection.Reverse);

            CircleCurve cm = (CircleCurve)cc1.Merge(cc2);

            Assert.IsNotNull(cm);
            Assert.AreEqual(new Vector2(), cm.Position);
            Assert.AreEqual(1, cm.Radius, 0);
            Assert.AreEqual(CircleCurve.RotationDirection.Reverse, cm.Rotation);
            Assert.AreEqual(0, cm.StartParam, 0);
            Assert.AreEqual(2 * Mathf.PI, cm.EndParam, 0);
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI, CircleCurve.RotationDirection.Forwards);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, Mathf.PI, 2 * Mathf.PI, CircleCurve.RotationDirection.Reverse);

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
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            CircleCurve cc2 = new CircleCurve(new Vector2(), 1, 3 * Mathf.PI / 2, 2 * Mathf.PI);

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
        Assert.AreEqual(2 * Mathf.PI* 3, cc2.Length, 1e-6);
    }

    [Test]
    public void TestComputeNormal()
    {
        CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);

        Assert.IsTrue((new Vector2(0, 1) - cc1.Normal(0)).magnitude < 1e-6);
        Assert.IsTrue((new Vector2(1, 0) - cc1.Normal(Mathf.PI / 2)).magnitude < 1e-6);
        Assert.IsTrue((new Vector2(0, -1) - cc1.Normal(Mathf.PI)).magnitude < 1e-6);
        Assert.IsTrue((new Vector2(-1, 0) - cc1.Normal(3 * Mathf.PI / 2)).magnitude < 1e-6);
    }

    [Test]
    public void TestWithinParams()
    {
        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI / 2);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI / 2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(Mathf.PI, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, 2 * Mathf.PI);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI / 2, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 3 * Mathf.PI / 2, 5 * Mathf.PI / 2);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI / 2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(Mathf.PI, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI / 2, CircleCurve.RotationDirection.Reverse);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI / 2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(Mathf.PI, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, 2 * Mathf.PI, CircleCurve.RotationDirection.Reverse);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI / 2, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(-1, 1e-6f));
        }

        {
            CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 3 * Mathf.PI / 2, 5 * Mathf.PI / 2,
                  CircleCurve.RotationDirection.Reverse);
            Assert.IsTrue(cc1.WithinParams(0, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(Mathf.PI / 2, 1e-6f));
            Assert.IsFalse(cc1.WithinParams(Mathf.PI, 1e-6f));
            Assert.IsTrue(cc1.WithinParams(-1, 1e-6f));
        }
    }

    [Test]
    public void TestIsCyclic()
    {
        CircleCurve cc1 = new CircleCurve(new Vector2(), 1, 0, 2 * Mathf.PI);
        CircleCurve cc2 = new CircleCurve(new Vector2(), 1, 0, 2 * Mathf.PI, CircleCurve.RotationDirection.Reverse);
        CircleCurve cc3 = new CircleCurve(new Vector2(), 1, Mathf.PI, 3 * Mathf.PI);
        CircleCurve cc4 = new CircleCurve(new Vector2(), 1, 0, 3 * Mathf.PI / 2);

        Assert.IsTrue(cc1.IsCyclic());
        Assert.IsTrue(cc2.IsCyclic());
        Assert.IsTrue(cc3.IsCyclic());
        Assert.IsFalse(cc4.IsCyclic());
    }
}

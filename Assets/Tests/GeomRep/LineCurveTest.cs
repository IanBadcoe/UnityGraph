using Assets.Generation.GeomRep;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LineCurveTest
{
    [Test]
    public void TestCtor()
    {
        {
            bool thrown = false;

            try
            {
                // non-unit direction
                new LineCurve(new Vector2(), new Vector2(1, 1), 1);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }
    }

    [Test]
    public void TestFindParamForPoint()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);

        Assert.AreEqual(0, lc.FindParamForPoint(new Vector2()), 1e-6f);
        Assert.AreEqual(1, lc.FindParamForPoint(new Vector2(1, 0)), 1e-6f);
        // using a point off the line still finds the closest point on the line
        Assert.AreEqual(1, lc.FindParamForPoint(new Vector2(1, 1)), 1e-6f);
        Assert.IsNull(lc.FindParamForPoint(new Vector2(-0.1f, 0)));
        Assert.IsNull(lc.FindParamForPoint(new Vector2(5.1f, 0)));
    }

    [Test]
    public void TestCloneWithChangedParams()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);

        LineCurve lc2 = (LineCurve)lc.CloneWithChangedParams(2, 4);

        Assert.IsNotNull(lc2);
        Assert.AreEqual(2, lc2.StartParam, 0);
        Assert.AreEqual(4, lc2.EndParam, 0);
        Assert.AreEqual(new Vector2(), lc2.Position);
        Assert.AreEqual(new Vector2(1, 0), lc2.Direction);
    }

    [Test]
    public void TestBoundingArea()
    {
        LineCurve lc = new LineCurve(new Vector2(-1, -2), new Vector2(1, 0), 5);
        LineCurve lc2 = new LineCurve(new Vector2(), new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), 5);
        LineCurve lc3 = new LineCurve(new Vector2(10, 11), new Vector2(0, 1), 5);

        Assert.AreEqual(new Box2(new Vector2(-1, -2), new Vector2(4, -2)), lc.BoundingArea);
        Assert.AreEqual(new Box2(new Vector2(), new Vector2(5 / Mathf.Sqrt(2), 5 / Mathf.Sqrt(2))), lc2.BoundingArea);
        Assert.AreEqual(new Box2(new Vector2(10, 11), new Vector2(10, 16)), lc3.BoundingArea);
    }

    [Test]
    public void TestTangent()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);
        LineCurve lc2 = new LineCurve(new Vector2(-1, -2), new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), 5);

        Assert.AreEqual(new Vector2(1, 0), lc.Tangent(0.0f));
        Assert.AreEqual(new Vector2(1, 0), lc.Tangent(1.0f));
        Assert.AreEqual(new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), lc2.Tangent(0.0f));
        Assert.AreEqual(new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), lc2.Tangent(1.0f));
    }

    [Test]
    public void TestMerge()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);

        {
            Curve c = new LineCurve(new Vector2(0, 0), new Vector2(1, 0), 5, 10);
            LineCurve lc3 = (LineCurve)lc.Merge(c);

            Assert.IsNotNull(lc3);
            Assert.AreEqual(new Vector2(), lc3.Position);
            Assert.AreEqual(new Vector2(1, 0), lc3.Direction);
            Assert.AreEqual(10, lc3.Length, 0);
            Assert.AreEqual(0, lc3.StartParam, 0);
            Assert.AreEqual(10, lc3.EndParam, 0);
        }

        {
            // does not work other way around, we need to supply the following curve to the followed
            // (could easily make that work, however, but current usage always knows the order...
            Curve c = new LineCurve(new Vector2(0, 0), new Vector2(1, 0), 5, 10);
            LineCurve lc3 = (LineCurve)c.Merge(lc);

            Assert.IsNull(lc3);
        }

        {
            // if the position and params are different, but the direction and
            // end-point still match up, then should merge
            Curve c = new LineCurve(new Vector2(5, 0), new Vector2(1, 0), 5);
            LineCurve lc3 = (LineCurve)lc.Merge(c);

            Assert.IsNotNull(lc3);
            Assert.AreEqual(new Vector2(), lc3.Position);
            Assert.AreEqual(new Vector2(1, 0), lc3.Direction);
            Assert.AreEqual(10, lc3.Length, 0);
            Assert.AreEqual(0, lc3.StartParam, 0);
            Assert.AreEqual(10, lc3.EndParam, 0);
        }

        // not with self
        {
            LineCurve lc3 = (LineCurve)lc.Merge(lc);

            Assert.IsNull(lc3);
        }

        // not with different curve type
        {
            Curve c = new CircleCurve(new Vector2(0, 0), 5, Mathf.PI / 2, 3 * Mathf.PI / 2);
            LineCurve lc3 = (LineCurve)lc.Merge(c);

            Assert.IsNull(lc3);
        }

        // for these two, could in fact merge if pos and param did not match, as long as
        // StartPos of one curve matched EndPos of the other
        // but current usage doesn't try that ATM...

        // not if position doesn't match
        // (we could here, detect if a different position nonetheless lies on the same line
        //  but so far we never need that case as we're only ever re-merging things we just split...)
        {
            Curve c = new LineCurve(new Vector2(0, 1), new Vector2(1, 0), 5, 10);
            LineCurve lc3 = (LineCurve)lc.Merge(c);

            Assert.IsNull(lc3);
        }

        // not if end and start params don't coincide
        {
            Curve c = new LineCurve(new Vector2(0, 0), new Vector2(1, 0), 6, 10);
            LineCurve lc3 = (LineCurve)lc.Merge(c);

            Assert.IsNull(lc3);
        }

        // not if different direction
        {
            Curve c = new LineCurve(new Vector2(0, 0), new Vector2(0, 1), 5, 10);
            LineCurve lc3 = (LineCurve)lc.Merge(c);

            Assert.IsNull(lc3);
        }
    }

    [Test]
    public void TestLength()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);
        LineCurve lc2 = new LineCurve(new Vector2(), new Vector2(1, 0), 6);
        LineCurve lc3 = new LineCurve(new Vector2(), new Vector2(1, 0), 5, 10);

        Assert.AreEqual(5, lc.Length, 0);
        Assert.AreEqual(6, lc2.Length, 0);
        Assert.AreEqual(5, lc3.Length, 0);
    }

    [Test]
    public void TestNormal()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);
        LineCurve lc2 = new LineCurve(new Vector2(-1, -2), new Vector2(1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), 5);

        Assert.AreEqual(new Vector2(0, 1), lc.Normal(0.0f));
        Assert.AreEqual(new Vector2(0, 1), lc.Normal(1.0f));

        Assert.AreEqual(new Vector2(-1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), lc2.Normal(0.0f));
        Assert.AreEqual(new Vector2(-1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2)), lc2.Normal(1.0f));
    }

    [Test]
    public void TestGetHashCode()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);
        LineCurve lcb = new LineCurve(new Vector2(), new Vector2(1, 0), 5);
        LineCurve lc2 = new LineCurve(new Vector2(1, 0), new Vector2(1, 0), 5);
        LineCurve lc3 = new LineCurve(new Vector2(), new Vector2(0, 1), 5);
        LineCurve lc4 = new LineCurve(new Vector2(), new Vector2(1, 0), 6);
        LineCurve lc5 = new LineCurve(new Vector2(), new Vector2(1, 0), 1, 5);

        Assert.AreEqual(lc.GetHashCode(), lcb.GetHashCode());
        Assert.AreNotEqual(lc.GetHashCode(), lc2.GetHashCode());
        Assert.AreNotEqual(lc.GetHashCode(), lc3.GetHashCode());
        Assert.AreNotEqual(lc.GetHashCode(), lc4.GetHashCode());
        Assert.AreNotEqual(lc.GetHashCode(), lc5.GetHashCode());
    }

    [Test]
    public void TestSpecialGetHashCode()
    {
        // lines with the same StartPos and EndPos are the same line,
        // even if the params and pos used to achieve that differ
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 0, 5);
        LineCurve lc2 = new LineCurve(new Vector2(-2, 0), new Vector2(1, 0), 2, 7);

        Assert.AreEqual(lc.GetHashCode(), lc2.GetHashCode());
    }

    [Test]
    public void TestEquals()
    {
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 5);
        LineCurve lcb = new LineCurve(new Vector2(), new Vector2(1, 0), 5);
        LineCurve lc2 = new LineCurve(new Vector2(1, 0), new Vector2(1, 0), 5);
        LineCurve lc3 = new LineCurve(new Vector2(), new Vector2(0, 1), 5);
        LineCurve lc4 = new LineCurve(new Vector2(), new Vector2(1, 0), 6);
        LineCurve lc5 = new LineCurve(new Vector2(), new Vector2(1, 0), 1, 5);

        //noinspection EqualsWithItself
        Assert.AreEqual(lc, lc);
        //noinspection EqualsBetweenInconvertibleTypes
        Assert.AreNotEqual(lc, 1);

        Assert.AreEqual(lc, lcb);
        Assert.AreNotEqual(lc, lc2);
        Assert.AreNotEqual(lc, lc3);
        Assert.AreNotEqual(lc, lc4);
        Assert.AreNotEqual(lc, lc5);
    }

    [Test]
    public void TestSpecialEquals()
    {
        // lines with the same StartPos and EndPos are equal, even if the position and params used to achieve that
        // are not
        LineCurve lc = new LineCurve(new Vector2(), new Vector2(1, 0), 0, 4);
        LineCurve lc2 = new LineCurve(new Vector2(-2, 0), new Vector2(1, 0), 2, 6);

        //noinspection EqualsWithItself
        Assert.AreEqual(lc, lc2);
    }
}

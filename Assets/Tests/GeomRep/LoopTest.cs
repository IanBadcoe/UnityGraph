using Assets.Generation.GeomRep;
using Assets.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LoopTest
{
    [Test]
    public void TestCtors()
    {
        // single fullcircle works
        {
            Curve c0 = new CircleCurve(new Vector2(), 1);

            new Loop("", c0);
        }

        // part circle fails (ends don't meet)
        {
            Curve c0 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);

            Assert.Throws<ArgumentException>(() =>new Loop("", c0));
        }

        // several parts that form a loop work
        {
            Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), 2);
            Curve c3 = new CircleCurve(new Vector2(-2, 0), 1, Mathf.PI, 2 * Mathf.PI);
            Curve c4 = new LineCurve(new Vector2(-2, 1), new Vector2(1, 0), 2);

            List<Curve> list = new List<Curve>();
            list.Add(c1);
            list.Add(c2);
            list.Add(c3);
            list.Add(c4);

            new Loop("", list);
        }

        // several parts that don't form a loop throw
        {
            Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), 2);
            Curve c3 = new CircleCurve(new Vector2(-2, 0), 1, Mathf.PI, 2 * Mathf.PI);

            List<Curve> list = new List<Curve>();
            list.Add(c1);
            list.Add(c2);
            list.Add(c3);

            Assert.Throws<ArgumentException>(() => new Loop("", list));
        }
    }

    [Test]
    public void TestParams()
    {
        // param range of loop is sum of ranges of curves
        {
            Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), 2);
            Curve c3 = new CircleCurve(new Vector2(-2, 0), 1, Mathf.PI, 2 * Mathf.PI);
            Curve c4 = new LineCurve(new Vector2(-2, 1), new Vector2(1, 0), 2);

            List<Curve> list = new List<Curve>();

            list.Add(c1);
            list.Add(c2);
            list.Add(c3);
            list.Add(c4);

            Loop l = new Loop("", list);

            Assert.AreEqual(6, l.ParamRange);
        }

        // worth trying again with c3 as first curve
        // as that means the first param of the first curve is non-zero, which the
        // loop adjusts for because its param always runs from zero
        {
            Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), 2);
            Curve c3 = new CircleCurve(new Vector2(-2, 0), 1, Mathf.PI, 2 * Mathf.PI);
            Curve c4 = new LineCurve(new Vector2(-2, 1), new Vector2(1, 0), 2);

            List<Curve> list = new List<Curve>();

            list.Add(c3);
            list.Add(c4);
            list.Add(c1);
            list.Add(c2);

            Loop l = new Loop("", list);

            Assert.AreEqual(6, l.ParamRange);
        }
    }

    [Test]
    public void TestPos()
    {
        Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
        Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), 2);
        Curve c3 = new CircleCurve(new Vector2(-2, 0), 1, Mathf.PI, 2 * Mathf.PI);
        Curve c4 = new LineCurve(new Vector2(-2, 1), new Vector2(1, 0), 2);

        List<Curve> list = new List<Curve>();

        list.Add(c1);
        list.Add(c2);
        list.Add(c3);
        list.Add(c4);

        Loop l = new Loop("", list);

        Vector2 c1_start = new Vector2(0, 1);
        Vector2 c1_mid = new Vector2(1, 0);
        Vector2 c2_start = new Vector2(0, -1);
        Vector2 c2_mid = new Vector2(-1, -1);
        Vector2 c3_start = new Vector2(-2, -1);
        Vector2 c3_mid = new Vector2(-3, 0);
        Vector2 c4_start = new Vector2(-2, 1);
        Vector2 c4_mid = new Vector2(-1, 1);

        Assert.IsTrue(c1_start.Equals(l.ComputePos(0).Value, 1e-6f));
        Assert.IsTrue(c1_mid.Equals(l.ComputePos(0.5f).Value, 1e-6f));
        Assert.IsTrue(c2_start.Equals(l.ComputePos(1).Value, 1e-6f));
        Assert.IsTrue(c2_mid.Equals(l.ComputePos(2).Value, 1e-6f));
        Assert.IsTrue(c3_start.Equals(l.ComputePos(3).Value, 1e-6f));
        Assert.IsTrue(c3_mid.Equals(l.ComputePos(3.5f).Value, 1e-6f));
        Assert.IsTrue(c4_start.Equals(l.ComputePos(4).Value, 1e-6f));
        Assert.IsTrue(c4_mid.Equals(l.ComputePos(5).Value, 1e-6f));
        Assert.IsTrue(c1_start.Equals(l.ComputePos(6).Value, 1e-6f));

        Assert.IsNull(l.ComputePos(100));
        Assert.IsNull(l.ComputePos(-1));
    }

    [Test]
    public void TestGetHashCode()
    {
        Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
        Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), 2);
        Curve c3 = new CircleCurve(new Vector2(-2, 0), 1, Mathf.PI, 2 * Mathf.PI);
        Curve c4 = new LineCurve(new Vector2(-2, 1), new Vector2(1, 0), 2);

        List<Curve> list = new List<Curve> {
            c1,
            c2,
            c3,
            c4,
        };

        // cyclic permutation of curves does not make us a different value
        // (although ComputePos will output positions in a different order)
        List<Curve> list_p = new List<Curve> {
            c2,
            c3,
            c4,
            c1,
        };

        Loop l1 = new Loop("", list);
        Loop l1b = new Loop("", list);
        Loop l1c = new Loop("", list_p);
        Loop l2 = new Loop("", new CircleCurve(new Vector2(), 1));

        Assert.AreEqual(l1.GetHashCode(), l1b.GetHashCode());
        Assert.AreEqual(l1.GetHashCode(), l1c.GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), l2.GetHashCode());
    }

    [Test]
    public void TestEquals()
    {
        Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
        Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), 2);
        Curve c3 = new CircleCurve(new Vector2(-2, 0), 1, Mathf.PI, 2 * Mathf.PI);
        Curve c4 = new LineCurve(new Vector2(-2, 1), new Vector2(1, 0), 2);

        List<Curve> list = new List<Curve> {
            c1,
            c2,
            c3,
            c4,
        };

        // cyclic permutation of curves does not make us a different value
        // (although ComputePos will output positions in a different order)
        List<Curve> list_p = new List<Curve> {
            c2,
            c3,
            c4,
            c1,
        };

        Loop l1 = new Loop("", list);
        Loop l1b = new Loop("", list);
        Loop l1c = new Loop("", list_p);
        Loop l2 = new Loop("", new CircleCurve(new Vector2(), 1));

        Assert.IsTrue(l1.Equals(l1b));
        Assert.IsTrue(l1 == l1b);
        Assert.IsTrue(l1.Equals(l1c));
        Assert.IsTrue(l1 == l1c);
        Assert.IsFalse(l1.Equals(l2));
        Assert.IsFalse(l1 == l2);
        //noinspection EqualsBetweenInconvertibleTypes
        Assert.IsFalse(l1.Equals(1));
        //noinspection EqualsWithItself
        Assert.IsTrue(l1.Equals(l1));
    }

    [Test]
   public void TestFacet()
    {
        {
            Loop l = new Loop("", new CircleCurve(new Vector2(), 1));

            List<Vector3> points = new List<Vector3>(l.Facet(Mathf.PI / 2));

            // circle radius is 2pi, so expect 4 points

            Assert.AreEqual(4, points.Count);
            Assert.IsTrue(new Vector3(0, 1, 0).Equals(points[0], 1e-6f));
            Assert.IsTrue(new Vector3(1, 0, 0).Equals(points[1], 1e-6f));
            Assert.IsTrue(new Vector3(0, -1, 0).Equals(points[2], 1e-6f));
            Assert.IsTrue(new Vector3(-1, 0, 0).Equals(points[3], 1e-6f));
        }

        {
            Curve c1 = new CircleCurve(new Vector2(), 1, 0, Mathf.PI);
            Curve c2 = new LineCurve(new Vector2(0, -1), new Vector2(-1, 0), Mathf.PI);
            Curve c3 = new CircleCurve(new Vector2(-Mathf.PI, 0), 1, Mathf.PI, 2 * Mathf.PI);
            Curve c4 = new LineCurve(new Vector2(-Mathf.PI, 1), new Vector2(1, 0), Mathf.PI);

            List<Curve> list = new List<Curve>();

            list.Add(c1);
            list.Add(c2);
            list.Add(c3);
            list.Add(c4);

            Loop l = new Loop("", list);

            List<Vector3> points = new List<Vector3>(l.Facet(Mathf.PI / 2));

            // capped rectagle is 4pi in total radius, so expect 8 points

            Assert.AreEqual(8, points.Count);
            Assert.IsTrue(new Vector3(0, 1).Equals(points[0], 1e-6f));
            Assert.IsTrue(new Vector3(1, 0).Equals(points[1], 1e-6f));
            Assert.IsTrue(new Vector3(0, -1).Equals(points[2], 1e-6f));
            Assert.IsTrue(new Vector3(-Mathf.PI / 2, -1).Equals(points[3], 1e-6f));
            Assert.IsTrue(new Vector3(-Mathf.PI, -1).Equals(points[4], 1e-6f));
            Assert.IsTrue(new Vector3(-Mathf.PI - 1, 0).Equals(points[5], 1e-6f));
            Assert.IsTrue(new Vector3(-Mathf.PI, 1).Equals(points[6], 1e-6f));
            Assert.IsTrue(new Vector3(-Mathf.PI / 2, 1).Equals(points[7], 1e-6f));
        }
    }
}

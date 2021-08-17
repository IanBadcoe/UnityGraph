using Assets.Generation.GeomRep;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

    [Test]
    public void TestNormDist()
    {
        {
            LineCurve lc1 = LineCurve.MakeFromPoints(new Vector2(0, 0), new Vector2(1, 0));
            LineCurve lc1r = LineCurve.MakeFromPoints(new Vector2(1, 0), new Vector2(0, 0));
            LineCurve lc1m = LineCurve.MakeFromPoints(new Vector2(1, 1), new Vector2(2, 1));
            LineCurve lc1mr = LineCurve.MakeFromPoints(new Vector2(2, 1), new Vector2(1, 1));

            LineCurve.NormalAndDistLineParams exp = new LineCurve.NormalAndDistLineParams(new Vector2(0, 1), 0);
            LineCurve.NormalAndDistLineParams expm = new LineCurve.NormalAndDistLineParams(new Vector2(0, 1), 1);

            Assert.AreEqual(exp, lc1.GetNormAndDistDescription());
            Assert.AreEqual(exp, lc1r.GetNormAndDistDescription());
            Assert.AreEqual(expm, lc1m.GetNormAndDistDescription());
            Assert.AreEqual(expm, lc1mr.GetNormAndDistDescription());
        }

        List<LineCurve> all_lc2s = new List<LineCurve>();

        for (int offset_x = 0; offset_x <= 1; offset_x += 1)
        {
            for (int offset_y = 0; offset_y <= 1; offset_y += 1)
            {
                int hx = 3;
                int hy = 4;

                foreach (bool neg in new List<bool> { true, false })
                {
                    if (neg)
                    {
                        hx = -hx;
                        hy = -hy;
                    }

                    List<LineCurve> lc2s = new List<LineCurve>
                    {
                        LineCurve.MakeFromPoints(new Vector2(offset_x, offset_y), new Vector2(hx + offset_x, hy + offset_y)),
                        LineCurve.MakeFromPoints(new Vector2(hx + offset_x, hy + offset_y), new Vector2(offset_x, offset_y))
                    };

                    var expected = lc2s[0].GetNormAndDistDescription();

                    foreach (var c in lc2s)
                    {
                        // reversal should never change result
                        Assert.IsTrue(expected.Equals(c.GetNormAndDistDescription(), 1e-4f));
                    }

                    all_lc2s.AddRange(lc2s);
                }
            }
        }

        {
            var expected = all_lc2s[0].GetNormAndDistDescription();

            foreach (var c in all_lc2s)
            {
                // translation, reversal and mirroring should never change normal
                Assert.IsTrue(
                    (expected.Normal - c.GetNormAndDistDescription().Normal).magnitude < 1e-4f
                    || (expected.Normal - -c.GetNormAndDistDescription().Normal).magnitude < 1e-4f);
            }
        }
    }

    [Test]
    public void TestCoaxial()
    {
        for (float ang = 0; ang < Mathf.PI * 2; ang += 0.1f)
        {
            float sk = Mathf.Sin(ang);
            float ck = Mathf.Cos(ang);

            Vector2 p1 = new Vector2(sk * 3, ck * 3);
            Vector2 p2 = new Vector2(sk * 5, ck * 5);
            Vector2 p3 = new Vector2(sk * 11, ck * 11);
            Vector2 p4 = new Vector2(sk * 103, ck * 103);

            var lc1 = LineCurve.MakeFromPoints(p1, p2);
            var lc1x = LineCurve.MakeFromPoints(p1 + new Vector2(ck, sk) * 0.01f, p2);

            var lc2 = LineCurve.MakeFromPoints(p3, p4);
            var lc3 = LineCurve.MakeFromPoints(p2, p4);

            Assert.IsTrue(lc1.SameSupercurve(lc2, 1e-4f));
            Assert.IsTrue(lc2.SameSupercurve(lc1, 1e-4f));
            Assert.IsTrue(lc1.SameSupercurve(lc3, 1e-4f));

            Assert.IsFalse(lc1.SameSupercurve(lc1x, 1e-4f));
        }
    }

    [Test]
    public void TestSplitCoincidentCurves()
    {
        for (float ang = 0; ang < Mathf.PI * 2; ang += 0.5f)
        {
            float sk = Mathf.Sin(ang);
            float ck = Mathf.Cos(ang);

            Vector2 p1 = new Vector2(sk * 10, ck * 10);
            Vector2 p2 = new Vector2(sk * 20, ck * 20);
            Vector2 p3 = new Vector2(sk * 30, ck * 30);
            Vector2 p4 = new Vector2(sk * 40, ck * 40);

            var lc1 = LineCurve.MakeFromPoints(p1, p3);
            var lc2 = LineCurve.MakeFromPoints(p2, p4);
            var lc3 = LineCurve.MakeFromPoints(p1, p4);
            var lc4 = LineCurve.MakeFromPoints(p2, p3);
            var lc1x = LineCurve.MakeFromPoints(p1 + new Vector2(ck, sk) * 0.1f, p2);

            Assert.IsNull(lc1.SplitCoincidentCurves(lc1x, 1e-4f));
            Assert.IsNull(lc2.SplitCoincidentCurves(lc1x, 1e-4f));
            Assert.IsNull(lc3.SplitCoincidentCurves(lc1x, 1e-4f));
            Assert.IsNull(lc4.SplitCoincidentCurves(lc1x, 1e-4f));
            //          3   5   7   9
            // line 1 : --------- 
            // line 2 :     ---------
            // line 3 : -------------
            // line 4 :     -----

            // null operation comes back null
            Assert.IsNull(lc1.SplitCoincidentCurves(lc1, 1e-4f));

            {
                var curves = lc1.SplitCoincidentCurves(lc2, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);
                Assert.AreEqual(2, curves.Item1.Count);
                Assert.AreEqual(2, curves.Item2.Count);

                CheckCurveSplit(lc1, new List<float> { 10 }, curves.Item1);
                CheckCurveSplit(lc2, new List<float> { 10 }, curves.Item2);
            }

            {
                var curves = lc1.SplitCoincidentCurves(lc3, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);
                Assert.AreEqual(2, curves.Item2.Count);

                CheckCurveSplit(lc3, new List<float> { 20 }, curves.Item2);
            }

            {
                var curves = lc2.SplitCoincidentCurves(lc3, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);
                Assert.AreEqual(2, curves.Item2.Count);

                CheckCurveSplit(lc3, new List<float> { 10 }, curves.Item2);
            }

            {
                var curves = lc3.SplitCoincidentCurves(lc4, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNull(curves.Item2);
                Assert.AreEqual(3, curves.Item1.Count);

                CheckCurveSplit(lc3, new List<float> { 10, 20 }, curves.Item1);
            }

            var lc1r = lc1.Reversed();
            var lc2r = lc2.Reversed();
            var lc3r = lc3.Reversed();
            var lc4r = lc4.Reversed();
            var lc1xr = lc1x.Reversed();

            Assert.IsNull(lc1r.SplitCoincidentCurves(lc1x, 1e-4f));
            Assert.IsNull(lc2r.SplitCoincidentCurves(lc1x, 1e-4f));
            Assert.IsNull(lc3r.SplitCoincidentCurves(lc1x, 1e-4f));
            Assert.IsNull(lc4r.SplitCoincidentCurves(lc1x, 1e-4f));

            Assert.IsNull(lc1.SplitCoincidentCurves(lc1xr, 1e-4f));
            Assert.IsNull(lc2.SplitCoincidentCurves(lc1xr, 1e-4f));
            Assert.IsNull(lc3.SplitCoincidentCurves(lc1xr, 1e-4f));
            Assert.IsNull(lc4.SplitCoincidentCurves(lc1xr, 1e-4f));

            Assert.IsNull(lc1.SplitCoincidentCurves(lc1, 1e-4f));
            Assert.IsNull(lc2.SplitCoincidentCurves(lc2, 1e-4f));
            Assert.IsNull(lc3.SplitCoincidentCurves(lc3, 1e-4f));
            Assert.IsNull(lc4.SplitCoincidentCurves(lc4, 1e-4f));

            {
                var curves = lc1.SplitCoincidentCurves(lc2r, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);
                Assert.AreEqual(2, curves.Item1.Count);
                Assert.AreEqual(2, curves.Item2.Count);

                CheckCurveSplit(lc1, new List<float> { 10 }, curves.Item1);
                CheckCurveSplit(lc2, new List<float> { 10 }, curves.Item2);
            }


            {
                var curves = lc1.SplitCoincidentCurves(lc3r, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);
                Assert.AreEqual(2, curves.Item2.Count);

                CheckCurveSplit(lc3, new List<float> { 10 }, curves.Item2);
            }

            {
                var curves = lc2.SplitCoincidentCurves(lc3r, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNull(curves.Item1);
                Assert.IsNotNull(curves.Item2);
                Assert.AreEqual(2, curves.Item2.Count);

                CheckCurveSplit(lc3, new List<float> { 20 }, curves.Item2);
            }

            {
                var curves = lc3.SplitCoincidentCurves(lc4r, 1e-4f);
                Assert.IsNotNull(curves);
                Assert.IsNotNull(curves.Item1);
                Assert.IsNull(curves.Item2);
                Assert.AreEqual(3, curves.Item1.Count);

                CheckCurveSplit(lc3, new List<float> { 10, 20 }, curves.Item1);
            }


            var lca = LineCurve.MakeFromPoints(p1, p2);
            var lcb = LineCurve.MakeFromPoints(p3, p4);

            {
                // coaxial but non-overlapping along the ray
                var parsab = lca.SplitCoincidentCurves(lcb, 1e-4f);
                Assert.IsNull(parsab);

                var parsba = lca.SplitCoincidentCurves(lcb, 1e-4f);
                Assert.IsNull(parsba);
            }
        }
    }

    private void CheckCurveSplit(LineCurve input, List<float> splits, IList<Curve> curves)
    {
        Assert.AreEqual(splits.Count + 1, curves.Count);

        Assert.AreEqual(input.StartParam, curves[0].StartParam, 1e-4f);
        Assert.AreEqual(input.EndParam, curves.Last().EndParam, 1e-4f);

        for (int i = 0; i < splits.Count; i++)
        {
            Assert.AreEqual(splits[i], curves[i].EndParam, 1e-4f);
            Assert.AreEqual(splits[i], curves[i + 1].StartParam, 1e-4f);
        }

        foreach (var c in curves)
        {
            var lc = c as LineCurve;
            Assert.IsNotNull(lc);
            Assert.IsTrue(input.SameSupercurve(lc, 1e-4f));
        }
    }
}

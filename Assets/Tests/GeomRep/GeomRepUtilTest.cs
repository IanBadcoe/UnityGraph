using Assets.Generation.GeomRep;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GeomRepUtilTest
{
    [Test]
    public void TestSignedArea()
    {
        {
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);
            Vector2 p3 = new Vector2(1, 1);
            Vector2 p4 = new Vector2(0, 1);

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3, p4 }, RotationDirection.Forwards);

                Assert.AreEqual(1.0f, GeomRepUtil.SignedPolygonArea(l));
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p2, p3, p4, p1 }, RotationDirection.Forwards);

                Assert.AreEqual(1.0f, GeomRepUtil.SignedPolygonArea(l));
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p4, p1, p2, p3 }, RotationDirection.Forwards);

                Assert.AreEqual(1.0f, GeomRepUtil.SignedPolygonArea(l));
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3 }, RotationDirection.Forwards);

                Assert.AreEqual(0.5f, GeomRepUtil.SignedPolygonArea(l), 1e-6f);
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p2, p3, p4 }, RotationDirection.Forwards);

                Assert.AreEqual(0.5f, GeomRepUtil.SignedPolygonArea(l), 1e-6f);
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p4, p1, p2 }, RotationDirection.Forwards);

                Assert.AreEqual(0.5f, GeomRepUtil.SignedPolygonArea(l), 1e-6f);
            }

            {
                Vector2 p1o = new Vector2(20, 10);
                Vector2 p2o = new Vector2(21, 10);
                Vector2 p3o = new Vector2(21, 11);
                Vector2 p4o = new Vector2(20, 11);

                Loop l = Loop.MakePolygon(new List<Vector2> { p4, p1, p2, p3 }, RotationDirection.Forwards);

                Assert.AreEqual(1.0f, GeomRepUtil.SignedPolygonArea(l));
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3, p4 }, RotationDirection.Reverse);

                Assert.AreEqual(-1.0f, GeomRepUtil.SignedPolygonArea(l));
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p2, p3, p4, p1 }, RotationDirection.Reverse);

                Assert.AreEqual(-1.0f, GeomRepUtil.SignedPolygonArea(l));
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p4, p1, p2, p3 }, RotationDirection.Reverse);

                Assert.AreEqual(-1.0f, GeomRepUtil.SignedPolygonArea(l));
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p1, p2, p3 }, RotationDirection.Reverse);

                Assert.AreEqual(-0.5f, GeomRepUtil.SignedPolygonArea(l), 1e-6f);
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p2, p3, p4 }, RotationDirection.Reverse);

                Assert.AreEqual(-0.5f, GeomRepUtil.SignedPolygonArea(l), 1e-6f);
            }

            {
                Loop l = Loop.MakePolygon(new List<Vector2> { p4, p1, p2 }, RotationDirection.Reverse);

                Assert.AreEqual(-0.5f, GeomRepUtil.SignedPolygonArea(l), 1e-6f);
            }

            {
                Vector2 p1o = new Vector2(20, 10);
                Vector2 p2o = new Vector2(21, 10);
                Vector2 p3o = new Vector2(21, 11);
                Vector2 p4o = new Vector2(20, 11);

                Loop l = Loop.MakePolygon(new List<Vector2> { p4, p1, p2, p3 }, RotationDirection.Reverse);

                Assert.AreEqual(-1.0f, GeomRepUtil.SignedPolygonArea(l));
            }
        }
    }
}

using Assets.Extensions;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public static class GeomRepUtil
    {
        public static List<Tuple<float, float>> CurveCurveIntersect(Curve c1, Curve c2, float tol = 1e-5f)
        {
            if (c1.Equals(c2))
            {
                return null;
            }

            Tuple<Vector2, Vector2?> pts;

            if (c1 is CircleCurve)
            {
                pts = CircleCurveIntersect((CircleCurve)c1, c2);
            }
            else if (c1 is LineCurve)
            {
                pts = LineCurveIntersect((LineCurve)c1, c2);
            }
            else
            {
                throw new NotSupportedException("Unknown type of curve");
            }

            if (pts == null)
            {
                return null;
            }

            List<Tuple<float, float>> ret = new List<Tuple<float, float>>();

            {
                float? pc1 = c1.FindParamForPoint(pts.Item1, tol);
                float? pc2 = c2.FindParamForPoint(pts.Item1, tol);

                if (pc1 != null && pc2 != null)
                {
                    ret.Add(new Tuple<float, float>(pc1.Value, pc2.Value));
                }
            }

            if (pts.Item2 != null)
            {
                float? pc1 = c1.FindParamForPoint(pts.Item2.Value, tol);
                float? pc2 = c2.FindParamForPoint(pts.Item2.Value, tol);

                if (pc1 != null && pc2 != null)
                {
                    ret.Add(new Tuple<float, float>(pc1.Value, pc2.Value));
                }
            }

            if (ret.Count > 0)
            {
                return ret;
            }

            return null;
        }

        public static RotationDirection GetPolygonDirection(Loop ret)
        {
            float area = SignedPolygonArea(ret);

            return area > 0 ? RotationDirection.Forwards :
                area < 0 ? RotationDirection.Reverse :
                RotationDirection.DontCare;
        }
        private static Tuple<Vector2, Vector2?> CircleCurveIntersect(CircleCurve c1, Curve c2)
        {
            if (c2 is CircleCurve)
            {
                return CircleCircleIntersect(c1, (CircleCurve)c2);
            }
            else if (c2 is LineCurve)
            {
                return CircleLineIntersect(c1, (LineCurve)c2);
            }

            throw new NotSupportedException("Unknown type of curve");
        }

        private static Tuple<Vector2, Vector2?> CircleCircleIntersect(CircleCurve c1, CircleCurve c2)
        {
            return Util.CircleCircleIntersect(c1.Position, c1.Radius, c2.Position, c2.Radius);
        }

        private static Tuple<Vector2, Vector2?> LineCurveIntersect(LineCurve c1, Curve c2)
        {
            if (c2 is CircleCurve)
            {
                return LineCircleIntersect(c1, (CircleCurve)c2);
            }
            else if (c2 is LineCurve)
            {
                Vector2? lli = LineLineIntersect(c1, (LineCurve)c2);

                if (lli != null)
                {
                    return new Tuple<Vector2, Vector2?>(lli.Value, null);
                }

                return null;
            }

            throw new NotSupportedException("Unknown type of curve");
        }

        private static Vector2? LineLineIntersect(LineCurve l1, LineCurve l2)
        {
            Tuple<float, float> ret = Util.EdgeIntersect(
                  l1.StartPos, l1.EndPos,
                  l2.StartPos, l2.EndPos);

            if (ret == null)
            {
                return null;
            }

            // inefficient, am going to calculate a position here, just so that I can
            // back-calculate params from it above, however line-line is the only intersection that gives
            // direct params so it would be a pain to change the approach for this one case

            // also "false" here as we need to return out-of-range values so we know they are off the end above
            return l1.Pos(l1.StartParam + l1.ParamRange * ret.Item1, false);
        }

        private static Tuple<Vector2, Vector2?> LineCircleIntersect(LineCurve l1, CircleCurve c2)
        {
            return CircleLineIntersect(c2, l1);
        }

        // algorithm stolen with thanks from:
        // http://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm
        private static Tuple<Vector2, Vector2?> CircleLineIntersect(CircleCurve c1, LineCurve l2)
        {
            Tuple<float, float?> ret = CircleLineIntersect(c1.Position, c1.Radius,
                  l2.StartPos, l2.EndPos);

            if (ret == null)
            {
                return null;
            }

            // "false" here as we need to return out-of-range values so we know they are off the end above
            Vector2 hit1 = l2.Pos(l2.StartParam + l2.ParamRange * ret.Item1, false);
            Vector2? hit2 = null;

            if (ret.Item2 != null)
            {
                hit2 = l2.Pos(l2.StartParam + l2.ParamRange * ret.Item2.Value, false);
            }

            return new Tuple<Vector2, Vector2?>(hit1, hit2);
        }

        public static Tuple<float, float?> CircleLineIntersect(Vector2 circlePos, float circleRadius,
                                                               Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 d = lineEnd - lineStart;
            Vector2 f = lineStart - circlePos;

            float a = d.sqrMagnitude;
            float b = 2 * f.Dot(d);
            float c = f.sqrMagnitude - circleRadius * circleRadius;

            float discriminant_2 = b * b - 4 * a * c;

            if (discriminant_2 < 0)
            {
                return null;
            }

            // ray didn't totally miss circle,
            // so there is a solution to
            // the equation.

            float discriminant = Mathf.Sqrt(discriminant_2);

            // either solution may be on or off the ray so need to test both
            // t1 is always the smaller value, because BOTH discriminant and
            // a are nonnegative.
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit),

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            float tol = 1e-5f;

            float hit1 = t1;
            float? hit2 = null;

            hit1 = t1;

            if (Math.Abs(t1 - t2) > tol)
            {
                hit2 = t2;
            }

            // we, in fact do not care about the param ranges here, as the caller checks that
            // using a tolerance we supply and which matters
            // so do not do the following checks
            // VVV

            //if (t1 >= -tol && t1 <= 1 + tol)
            //{
            //    hit1 = t1;
            //}

            //if (t2 >= -tol && t2 <= 1 + tol)
            //{
            //    hit2 = t2;
            //}

            //if (hit1 == null)
            //{
            //    hit1 = hit2;
            //    hit2 = null;
            //}

            // ^^^

            return new Tuple<float, float?>(hit1, hit2);
        }

        // only works for loops made of LineCurve
        public static float SignedPolygonArea(Loop l)
        {
            return SignedPolygonArea(l.Curves);
        }

        public static float SignedPolygonArea(ICollection<Curve> curves)
        {
            float ret = 0;

            foreach (var c in curves)
            {
                LineCurve lc = c as LineCurve;

                Assertion.Assert(lc != null);

                ret += (lc.StartPos.y + lc.EndPos.y) * (lc.EndPos.x - lc.StartPos.x);
            }

            return ret / 2;
        }

        public static float DistFromLine(Vector2 l1, Vector2 l2, Vector2 p)
        {
            var rel = p - l1;
            var dir = (l2 - l1).normalized;

            return rel.Dot(dir.Rot90());
        }
    }
}

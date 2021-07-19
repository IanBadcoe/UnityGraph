using Assets.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.U
{
    public static class Util
    {
        public static Tuple<float, float> EdgeIntersect(Vector2 edge1Start, Vector2 edge1End, Vector2 edge2Start, Vector2 edge2End)
        {
            return EdgeIntersect(
                  edge1Start.x, edge1Start.y,
                  edge1End.x, edge1End.y,
                  edge2Start.x, edge2Start.y,
                  edge2End.x, edge2End.y
            );
        }

        private static Tuple<float, float> EdgeIntersect(float edge1StartX, float edge1StartY,
                                                         float edge1EndX, float edge1EndY,
                                                         float edge2StartX, float edge2StartY,
                                                         float edge2EndX, float edge2EndY)
        {
            float den = (edge2EndX - edge2StartX) * (edge1StartY - edge1EndY) - (edge1StartX - edge1EndX) * (edge2EndY - edge2StartY);

            // very near to parallel
            if (Mathf.Abs(den) < 1e-20)
                return null;

            float t1 = ((edge2StartY - edge2EndY) * (edge1StartX - edge2StartX) + (edge2EndX - edge2StartX) * (edge1StartY - edge2StartY)) / den;

            if (t1 < 0 || t1 > 1)
                return null;

            float t2 = ((edge1StartY - edge1EndY) * (edge1StartX - edge2StartX) + (edge1EndX - edge1StartX) * (edge1StartY - edge2StartY)) / den;

            if (t2 < 0 || t2 > 1)
                return null;

            return new Tuple<float,float>(t1, t2);
        }

        public static Tuple<Vector2, Vector2?> CircleCircleIntersect(Vector2 c1, float r1, Vector2 c2, float r2)
        {
            float dist_2 = (c1 - c2).sqrMagnitude;
            float dist = Mathf.Sqrt(dist_2);

            // too far apart
            if (dist > r1 + r2)
                return null;

            // too close together
            if (dist < Mathf.Abs(r1 - r2))
                return null;

            float a = c1.x;
            float b = c1.y;
            float c = c2.x;
            float d = c2.y;

            float delta_2 = (dist + r1 + r2)
                  * (dist + r1 - r2)
                  * (dist - r1 + r2)
                  * (-dist + r1 + r2);

            // should have assured delta_2 +ve with the ifs above...
            // but rounding can give v. small negative numbers
            Assertion.Assert(delta_2 > -1e-6f);

            if (delta_2 < 0)
                delta_2 = 0;

            float delta = 0.25f * Mathf.Sqrt(delta_2);

            float xi1 = (a + c) / 2
                  + (c - a) * (r1 * r1 - r2 * r2) / (2 * dist_2)
                  + 2 * (b - d) * delta / dist_2;
            float xi2 = (a + c) / 2
                  + (c - a) * (r1 * r1 - r2 * r2) / (2 * dist_2)
                  - 2 * (b - d) * delta / dist_2;

            float yi1 = (b + d) / 2
                  + (d - b) * (r1 * r1 - r2 * r2) / (2 * dist_2)
                  - 2 * (a - c) * delta / dist_2;
            float yi2 = (b + d) / 2
                  + (d - b) * (r1 * r1 - r2 * r2) / (2 * dist_2)
                  + 2 * (a - c) * delta / dist_2;

            Vector2 p1 = new Vector2(xi1, yi1);

            Vector2? p2 = null;

            if (delta > 1e-6)
            {
                p2 = new Vector2(xi2, yi2);
            }

            return new Tuple<Vector2, Vector2?>(p1, p2);
        }

        public static float Atan2(Vector2 vec)
        {
            // Unity call these y and x, in tat order, but they have zero at 3 o'clock where I have it at 12 0'clock
            return Mathf.Atan2(vec.x, vec.y);
        }

        // removes any positive or negative whole turns to leave a number
        // between 0.0 and 2 PI
        public static float FixupAngle(float a)
        {
            while (a < 0)
                a += Mathf.PI * 2;

            while (a >= Mathf.PI * 2)
                a -= Mathf.PI * 2;

            return a;
        }

        public static bool ClockAwareAngleCompare(float a1, float a2, float tol)
        {
            float diff = FixupAngle(Mathf.Abs(a1 - a2));

            return diff <= tol || diff >= Mathf.PI * 2 - tol;
        }

        public static T RemoveRandom<T>(ClRand random, List<T> col)
        {
            int which = (int)(random.Nextfloat() * col.Count);

            var ret = col[which];
            col.RemoveAt(which);

            return ret;
        }

        public class NEDRet
        {
            public readonly float Dist;
            public readonly Vector2 Target;  // point of closest approach of Node to Edge
            public readonly Vector2 Direction;  // direction from Node to Target

            public NEDRet(float dist,
                   Vector2 target,
                   Vector2 direction)
            {
                Dist = dist;
                Target = target;
                Direction = direction;
            }
        }

        // specialised version returning extra data for use in force calculations
        // force calculation cannot  handle zero distances so returns null for that
        public static NEDRet NodeEdgeDistDetailed(Vector2 n,
                                                  Vector2 es,
                                                  Vector2 ee)
        {
            // direction and length of edge
            Vector2 de = ee - es;

            float le = de.magnitude;
            // don't expect to see and hope other forces will pull the ends apart
            if (le == 0.0f)
                return null;

            de = de / le;

            // line from n to edge start
            Vector2 dnes = n - es;

            // project that line onto the edge direction
            float proj = de.Dot(dnes);

            Vector2 t;
            if (proj < 0)
            {
                // closest approach before edge start
                t = es;
            }
            else if (proj < le)
            {
                // closest approach between edges
                t = es + de * proj;
            }
            else
            {
                // closest approach beyond edge end
                t = ee;
            }

            Vector2 d = t - n;

            float l = d.magnitude;

            // don't expect to see and hope other forces will pull the edge and node apart
            if (l == 0)
                return null;

            d = d / l;

            return new NEDRet(l, t, d);
        }
    }
}

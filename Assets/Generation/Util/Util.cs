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

    //    public static OrderedPair<Vector2, Vector2> circleCircleIntersect(Vector2 c1, float r1, Vector2 c2, float r2)
    //    {
    //        float dist_2 = c1.minus(c2).length2();
    //        float dist = Math.sqrt(dist_2);

    //        // too far apart
    //        if (dist > r1 + r2)
    //            return null;

    //        // too close together
    //        if (dist < Math.abs(r1 - r2))
    //            return null;

    //        float a = c1.X;
    //        float b = c1.Y;
    //        float c = c2.X;
    //        float d = c2.Y;

    //        float delta_2 = (dist + r1 + r2)
    //              * (dist + r1 - r2)
    //              * (dist - r1 + r2)
    //              * (-dist + r1 + r2);

    //        // should have assured delta_2 +ve with the ifs above...
    //        // but rounding can give v. small negative numbers
    //        assert delta_2 > -1e-6;

    //        if (delta_2 < 0)
    //            delta_2 = 0;

    //        float delta = 0.25 * Math.sqrt(delta_2);

    //        float xi1 = (a + c) / 2
    //              + (c - a) * (r1 * r1 - r2 * r2) / (2 * dist_2)
    //              + 2 * (b - d) * delta / dist_2;
    //        float xi2 = (a + c) / 2
    //              + (c - a) * (r1 * r1 - r2 * r2) / (2 * dist_2)
    //              - 2 * (b - d) * delta / dist_2;

    //        float yi1 = (b + d) / 2
    //              + (d - b) * (r1 * r1 - r2 * r2) / (2 * dist_2)
    //              - 2 * (a - c) * delta / dist_2;
    //        float yi2 = (b + d) / 2
    //              + (d - b) * (r1 * r1 - r2 * r2) / (2 * dist_2)
    //              + 2 * (a - c) * delta / dist_2;

    //        Vector2 p1 = new Vector2(xi1, yi1);

    //        Vector2 p2 = null;

    //        if (delta > 1e-6)
    //        {
    //            p2 = new Vector2(xi2, yi2);
    //        }

    //        return new OrderedPair<>(p1, p2);
    //    }

    //    public static float atan2(Vector2 vec)
    //    {
    //        //noinspection SuspiciousNameCombination
    //        return Math.atan2(vec.X, vec.Y);
    //    }

    //    // removes any positive or negative whole turns to leave a number
    //    // between 0.0 and 2 PI
    //    public static float fixupAngle(float a)
    //    {
    //        while (a < 0)
    //            a += Math.PI * 2;

    //        while (a >= Math.PI * 2)
    //            a -= Math.PI * 2;

    //        return a;
    //    }

    //    public static boolean clockAwareAngleCompare(float a1, float a2, float tol)
    //    {
    //        float diff = fixupAngle(Math.abs(a1 - a2));

    //        return diff <= tol || diff >= Math.PI * 2 - tol;
    //    }

    //    // d1 is the unit direction vector for the reference line
    //    // d2 is the unit direction vector for the line whose angle we're measuring
    //    static float relativeAngle(Vector2 d1, Vector2 d2)
    //    {
    //        float rel_y = d1.dot(d2);
    //        float rel_x = d1.rot90().dot(d2);

    //        //noinspection SuspiciousNameCombination
    //        return fixupAngle(Math.atan2(rel_x, rel_y));
    //    }
    }
}

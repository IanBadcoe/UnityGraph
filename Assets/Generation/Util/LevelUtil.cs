using Assets.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.Util
{
    public static class LevelUtil
    {
        /**
         * Calculate the force and distortion of an edge constrained to be between dmin and dmax in length.
         *
         * @param l    the current length of the edge
         * @param dmin the minimum permitted length of the edge
         * @param dmax the maximum permitted length of the edge
         * @return a pair of floats, the first is the fractional distortion of the edge.  If between dmin and dmax this
         * is 1.0 (no distortion) if shorter than dmin this is l as a fraction of dmin (< 1.0) and if
         * if longer than dmax then this is l as a fraction of dmax )e.g. > 1.0)
         * <p>
         * The second float is the force.  The sign of the force is that -ve is repulsive (happens when too close)
         * and vice versa.
         */
        public static Vector2 UnitEdgeForce(float l, float dmin, float dmax)
        {
            float ratio;

            // between min and max there is no force and we always return 1.0
            if (l < dmin)
            {
                ratio = l / dmin;
            }
            else if (l > dmax)
            {
                ratio = l / dmax;
            }
            else
            {
                ratio = 1.0f;
            }

            float force = (ratio - 1);

            return new Vector2(ratio, force);
        }

        /**
         * Calculate force and distance ratio of two circular nodes
         * @param l node separation
         * @param summed_radii idea minimum separation
         * @return a pair of floats, the first is a fractional measure of how much too close the nodes are,
         * zero if they are more than their summed_radii apart.
         * <p>
         * The second float is the force.  The sign of the force is that -ve is repulsive (happens when too close)
         * the are no attractive forces for nodes so the force is never > 0.
         */
        public static Vector2 UnitNodeForce(float l, float summed_radii)
        {
            float ratio = l / summed_radii;

            // no attractive forces
            if (ratio > 1)
            {
                return new Vector2(0.0f, 0.0f);
            }

            float force = (ratio - 1);

            // at the moment the relationship between force and overlap is trivial
            // but will keep the two return values in case the force develops a squared term or something...
            return new Vector2(-force, force);
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
        public static NEDRet nodeEdgeDistDetailed(Vector2 n,
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

        //  static float nodeEdgeDist(Vector2 n,
        //                             Vector2 es,
        //                             Vector2 ee)
        //  {
        //      // direction and length of edge
        //      Vector2 de = ee.minus(es);

        //      // don't expect to see and hope other forces will pull the ends apart
        //      if (de.isZero())
        //          throw new UnsupportedOperationException("zero length edge");

        //      float le = de.length();
        //      de = de.divide(le);

        //      // line from n to edge start
        //      Vector2 dnes = n.minus(es);

        //      // project that line onto the edge direction
        //      float proj = de.dot(dnes);

        //      Vector2 t;
        //      if (proj < 0)
        //      {
        //          // closest approach before edge start
        //          t = es;
        //      }
        //      else if (proj < le)
        //      {
        //          // closest approach between edges
        //          t = es.plus(de.multiply(proj));
        //      }
        //      else
        //      {
        //          // closest approach beyond edge end
        //          t = ee;
        //      }

        //      Vector2 d = t.minus(n);

        //      return d.length();
        //  }

        public static T RemoveRandom<T>(System.Random random, List<T> col)
        {
            int which = (int)(random.NextDouble() * col.Count);

            var ret = col[which];
            col.RemoveAt(which);

            return ret;
        }
    }
}

using Assets.Extensions;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    [System.Diagnostics.DebuggerDisplay("Curves = {NumCurves}")]
    public class Loop : EqualityBase
    {
        private readonly List<Curve> m_curves = new List<Curve>();
        public readonly float ParamRange;
        public readonly string Layer;

        // only used in unit-tests atm
        public Loop(string layer)
        {
            ParamRange = 0;
            Layer = layer;
        }

        // only used in unit-tests atm
        public Loop()
        {
            ParamRange = 0;
            Layer = "";
        }

        public Loop(string layer, Curve c)
            : this(layer)
        {
            m_curves.Add(c);

            ParamRange = c.EndParam - c.StartParam;

            Vector2 s = c.StartPos;
            Vector2 e = c.EndPos;

            if (!s.Equals(e, 1e-4f))
            {
                throw new ArgumentException("Curves do not form a closed loop");
            }
        }

        public Loop(string layer, IEnumerable<Curve> curves)
            : this(layer)
        {
            m_curves.AddRange(curves);

            float range = 0.0f;

            Curve prev = m_curves[m_curves.Count - 1];

            foreach (Curve curr in m_curves)
            {
                range += curr.ParamRange;

                Vector2 c_start = curr.StartPos;
                Vector2 p_end = prev.EndPos;

                if (!c_start.Equals(p_end, 1e-4f))
                {
                    throw new ArgumentException("Curves do not form a closed loop");
                }

                prev = curr;
            }

            ParamRange = range;
        }

        public static Loop MakeRect(float x1, float y1, float x2, float y2, string layer = "")
        {
            Assertion.Assert(x1 <= x2);
            Assertion.Assert(y1 <= y2);

            var c1 = new Vector2(x1, y1);
            var c2 = new Vector2(x1, y2);
            var c3 = new Vector2(x2, y2);
            var c4 = new Vector2(x2, y1);

            return new Loop(
                layer,
                new List<Curve> {
                    LineCurve.MakeFromPoints(c1, c2),
                    LineCurve.MakeFromPoints(c2, c3),
                    LineCurve.MakeFromPoints(c3, c4),
                    LineCurve.MakeFromPoints(c4, c1),
                });
        }

        public static Loop MakePolygon(IEnumerable<Vector2> pnts, RotationDirection polarity, string layer = "")
        {
            Vector2 prev = pnts.Last();

            List<Curve> curves = new List<Curve>();

            foreach (var curr in pnts)
            {
                curves.Add(LineCurve.MakeFromPoints(prev, curr));

                prev = curr;
            }

            Loop ret = new Loop(layer, curves);

            RotationDirection actual_rotation = GeomRepUtil.GetPolygonDirection(ret);

            if (polarity != RotationDirection.DontCare
                && actual_rotation != RotationDirection.DontCare
                && polarity != actual_rotation)
            {
                ret = ret.Reversed();
            }

            return ret;
        }

        public Loop Reversed()
        {
            List<Curve> ret = new List<Curve>();

            foreach (var c in m_curves)
            {
                ret.Insert(0, c.Reversed());
            }

            return new Loop(Layer, ret);
        }

        public Vector2? ComputePos(float p)
        {
            // because we don't use the curves eithinParams call
            // this routine should give the same behaviour for
            // multi-part curves and circles, even though the latter
            // just go round and round for any level of param
            if (p < 0)
            {
                return null;
            }

            // curve param ranges can be anywhere
            // but the loop param range starts from zero
            foreach (Curve c in m_curves)
            {
                if (c.ParamRange < p)
                {
                    p -= c.ParamRange;
                }
                else
                {
                    // shift the param range where the curve wants it...
                    // and we already fixed the range
                    return c.Pos(p + c.StartParam, false);
                }
            }

            return null;
        }

        public int NumCurves
        {
            get => m_curves.Count;
        }

        public ReadOnlyCollection<Curve> Curves
        {
            get => new ReadOnlyCollection<Curve>(m_curves);
        }
        public bool IsEmpty {
            get => m_curves.Count == 0;
        }

        public override int GetHashCode()
        {
            int h = 0;

            // standardise the curve order
            foreach (Curve c in CyclicPermuteCurves(m_curves))
            {
                h ^= c.GetHashCode();
                h *= 3;
            }

            h ^= Layer.GetHashCode();

            // m_param_range is derivative from the curves
            // so not required in hash

            return h;
        }

        public bool Equals(object o, float tol)
        {
            if (ReferenceEquals(o, this))
            {
                return true;
            }

            if (!(o is Loop))
            {
                return false;
            }

            Loop loop_o = (Loop)o;

            if (NumCurves != loop_o.NumCurves)
            {
                return false;
            }

            var my_curves_ordered = CyclicPermuteCurves(m_curves);
            var loop_o_curves_ordered = CyclicPermuteCurves(loop_o.m_curves);

            for (int i = 0; i < NumCurves; i++)
            {
                if (!my_curves_ordered[i].Equals(loop_o_curves_ordered[i], tol))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object o)
        {
            return Equals(o, 0.0f);
        }

        private static IList<Curve> CyclicPermuteCurves(IEnumerable<Curve> curves)
        {
            int hash = int.MaxValue;

            foreach (var c in curves)
            {
                hash = Math.Min(c.GetHashCode(), hash);
            }

            int which = curves.TakeWhile(x => x.GetHashCode() != hash).Count();

            return curves.Skip(which).Concat(curves.Take(which)).ToList();
        }

        public Vector3[] Facet(float max_length)
        {
            List<Vector3> temp = new List<Vector3>();

            foreach (Curve c in m_curves)
            {
                float param_step = c.ParamRange
                    * (max_length / c.Length);

                float p = 0;

                float start_p = c.StartParam;

                while (p < c.ParamRange)
                {
                    temp.Add(c.Pos(start_p + p, false));

                    p += param_step;
                }
            }

            return temp.ToArray();
        }

        public List<Tuple<Vector2, Vector2>> FacetWithNormals(float max_length)
        {
            List<Tuple<Vector2, Vector2>> ret = new List<Tuple<Vector2, Vector2>>();

            foreach (Curve c in m_curves)
            {
                // every bit of curve, however small, gets one facet
                // possibly could relax this later and use a global num steps for whole loop
                // but nice feature of this approach is it keeps any twiddly little steps we put in
                // it wouldn't keep a tiny little semi-circle
                // to do that we'd need to do at least two facets and/or take sharpness of curvature
                // into account
                int steps = (int)(c.Length / max_length) + 1;

                float param_step = c.ParamRange / steps;

                float p = c.StartParam;

                for (int i = 0; i < steps; i++)
                {
                    // curve normals point out into the area outside the playable regions
                    // we need wall normals to point in, so negate
                    //
                    // we take the normal 1/2 way to the next point, as (i) that is the middle of this segment and
                    // (ii) that won't get swung around at the end of this curve where the last point (and it's normal)
                    // is from the next curve
                    ret.Add(new Tuple<Vector2, Vector2>(
                        c.Pos(p, false),
                        -c.Normal(p + param_step / 2))
                    );

                    p += param_step;
                }
            }

            return ret;
        }

        public Box2 GetBounds()
        {
            Box2 ret = new Box2();

            foreach (var c in m_curves)
            {
                ret = ret.Union(c.BoundingArea);
            }

            return ret;
        }
    }
}

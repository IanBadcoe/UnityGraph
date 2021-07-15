﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Extensions;
using System.Collections.ObjectModel;

namespace Assets.Generation.GeomRep
{
    public class Loop
    {
        private readonly List<Curve> m_curves = new List<Curve>();
        private readonly float m_param_range;

        // only used in unit-tests atm
        public Loop()
        {
            m_param_range = 0;
        }

        public Loop(Curve c)
        {
            m_curves.Add(c);

            m_param_range = c.EndParam - c.StartParam;

            Vector2 s = c.StartPos();
            Vector2 e = c.EndPos();

            if (!s.Equals(e, 1e-4f))
                throw new ArgumentException("Curves do not form a closed loop");
        }

        public Loop(IList<Curve> curves)
        {
            m_curves.AddRange(curves);

            float range = 0.0f;

            Curve prev = m_curves[m_curves.Count - 1];

            foreach (Curve curr in m_curves)
            {
                range += curr.EndParam - curr.StartParam;

                Vector2 c_start = curr.StartPos();
                Vector2 p_end = prev.EndPos();

                if (!c_start.Equals(p_end, 1e-4f))
                    throw new ArgumentException("Curves do not form a closed loop");

                prev = curr;
            }

            m_param_range = range;
        }

        public float paramRange()
        {
            return m_param_range;
        }

        public Vector2? ComputePos(float p)
        {
            // because we don't use the curves eithinParams call
            // this routine should give the same behaviour for
            // multi-part curves and circles, even though the latter
            // just go round and round for any level of param
            if (p < 0)
                return null;

            // curve param ranges can be anywhere
            // but the loop param range starts from zero
            foreach (Curve c in m_curves)
            {
                if (c.ParamRange() < p)
                {
                    p -= c.ParamRange();
                }
                else
                {
                    // shift the param range where the curve wants it...
                    return c.ComputePos(p + c.StartParam);
                }
            }

            return null;
        }

        public int NumCurves()
        {
            return m_curves.Count;
        }

        public ReadOnlyCollection<Curve> GetCurves()
        {
            return new ReadOnlyCollection<Curve>(m_curves);
        }

        public override int GetHashCode()
        {
            int h = 0;

            foreach (Curve c in m_curves)
            {
                h ^= c.GetHashCode();
                h *= 3;
            }

            // m_param_range is derivative from the curves
            // so not required in hash

            return h;
        }

        public override bool Equals(object o)
        {
            if (o == this)
                return true;

            if (!(o is Loop))
                return false;

            Loop loop_o = (Loop)o;

            if (NumCurves() != loop_o.NumCurves())
                return false;

            for (int i = 0; i < NumCurves(); i++)
            {
                if (!m_curves[i].Equals(loop_o.m_curves[i]))
                    return false;
            }

            return true;
        }

        List<Vector2> Facet(float max_length)
        {
            List<Vector2> ret = new List<Vector2>();

            foreach(Curve c in m_curves)
            {
                float param_step = c.ParamRange()
                    * (max_length / c.Length());

                float p = 0;

                float start_p = c.StartParam;

                while(p < c.ParamRange())
                {
                    ret.Add(c.ComputePos(start_p + p));

                    p += param_step;
                }
            }

            return ret;
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
                int steps = (int)(c.Length() / max_length) + 1;

                float param_step = c.ParamRange() / steps;

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
                        c.ComputePos(p),
                        -c.ComputeNormal(p + param_step / 2))
                    );

                    p += param_step;
                }
            }

            return ret;
        }

        public Area GetBounds()
        {
            if (m_curves.Count == 0)
                return new Area();

            Area ret = new Area();

            foreach(var c in m_curves)
            {
                ret = ret.Union(c.BoundingArea());
            }

            return ret;
        }
    }
}
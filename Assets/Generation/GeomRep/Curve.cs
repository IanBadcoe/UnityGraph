using Assets.Extensions;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public abstract class Curve : EqualityBase
    {
        public readonly float StartParam;
        public readonly float EndParam;

        public abstract Box2 BoundingArea { get; }

        // geometric length between start and end params
        public abstract float Length { get; }

        protected Curve(float startParam, float endParam)
        {
            StartParam = startParam;
            EndParam = endParam;
        }

        public Vector2 StartPos
        {
            get => ComputePos_Inner(StartParam);
        }

        public Vector2 EndPos
        {
            get => ComputePos_Inner(EndParam);
        }

        public float ParamRange
        {
            get => EndParam - StartParam;
        }

        // exquisite abstractions

        public Vector2 Pos(float p, bool clamp_to_param_range)
        {
            if (clamp_to_param_range)
            {
                p = ClampToParamRange(p);
            }

            return ComputePos_Inner(p);
        }

        public abstract Vector2 Tangent(float param);

        public abstract Vector2 Normal(float p);

        public float? FindParamForPoint(Vector2 pnt, float tol = 1e-5f)
        {
            float ret = FindParamForPoint_Inner(pnt);

            if (!WithinParams(ret, tol))
            {
                return null;
            }

            //// forget why I put this in, but I was getting way-off positions at some point...
            //// so check quite a loose tolerance...
            //
            // we now allow the point to be off the curve
            //Assertion.Assert((ComputePos(ret) - pnt).magnitude < 1e-3f);

            return ret;
        }

        // differs from the above, in that if the point is off the end of the line, we just return the end-point
        // used for finding coincident portions of coaxial lines
        public float FindParamForPoint_Clamped(Vector2 pnt)
        {
            float ret = FindParamForPoint_Inner(pnt);

            ret = Mathf.Clamp(ret, StartParam, EndParam);

            return ret;
        }

        public abstract Curve Reversed();

        // this can return values off the end of our param range, but the caller checks that
        protected abstract float FindParamForPoint_Inner(Vector2 pnt);

        // for a LineCurve this changes the extents along the line in space
        // and also changes the params (since the params reflect distances from StartPos, and we don't move
        // StartPos (we could that that, and in theory everything would still work...)
        //
        // for a CircleCurve params are always mapped to 0 -> 1, so this doesn't change the params
        // it just alters the angles they map to
        public abstract Curve CloneWithChangedExtents(float start_param, float end_param);

        public abstract Curve Merge(Curve c_after);

        // overridden for cyclic curves

        public virtual bool WithinParams(float p, float tol)
        {
            return p > StartParam - tol
                  && p < EndParam + tol;
        }

        // overridden but overrides need to call these base implementations
        public override abstract int GetHashCode();
        protected int GetHashCode_Inner()
        {
            return StartParam.GetHashCode() + EndParam.GetHashCode() * 31;
        }

        public abstract bool Equals(Curve c, float tol);

        public override bool Equals(object o)
        {
            Curve c = o as Curve;

            if (c == null)
            {
                return false;
            }

            return Equals(c, 0.0f);
        }

        public float ParamCoordinateDist(float p1, float p2)
        {
            return (Pos(p1, true) - Pos(p2, true)).magnitude;
        }

        private float ClampToParamRange(float p)
        {
            return Mathf.Clamp(p, StartParam, EndParam);
        }

        protected abstract Vector2 ComputePos_Inner(float param);

        public bool Adjoins(Curve c2, float tol)
        {
            return StartPos.Equals(c2.EndPos, tol) || EndPos.Equals(c2.StartPos, tol);
        }

        public abstract Tuple<IList<Curve>, IList<Curve>> SplitCoincidentCurves(Curve c2, float tol);

        public abstract bool SameSupercurve(Curve curve, float tol);
    }
}

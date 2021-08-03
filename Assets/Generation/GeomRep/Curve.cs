using Assets.Extensions;
using Assets.Generation.U;
using System;
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

        public Vector2 StartPos
        {
            get => Pos(StartParam);
        }

        public Vector2 EndPos
        {
            get => Pos(EndParam);
        }

        public float ParamRange
        {
            get => EndParam - StartParam;
        }

        protected Curve(float start_param, float end_param)
        {
            StartParam = start_param;
            EndParam = end_param;

            if (EndParam - StartParam < 1e-12)
            {
                throw new NotSupportedException("StartParam must be < EndParam");
            }
        }

        // exquisite abstractions

        public Vector2 Pos(float p)
        {
            p = ClampToParamRange(p);

            return ComputePos_Inner(p);
        }

        public abstract Vector2 Tangent(float param);

        public abstract Vector2 Normal(float p);

        public float? FindParamForPoint(Vector2 pnt)
        {
            float ret = FindParamForPoint_Inner(pnt);

            if (!WithinParams(ret, 1e-5f))
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

        // this can return values off the end of our param range, but the caller checks that
        protected abstract float FindParamForPoint_Inner(Vector2 pnt);

        public abstract Curve CloneWithChangedParams(float start, float end);

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

        public override abstract bool Equals(object o);
        protected bool Equals_Inner(object o)
        {
            // need caller to have checked these
            Assertion.Assert(o is Curve);
            Assertion.Assert(!ReferenceEquals(o, this));

            Curve co = (Curve)o;

            return co.StartParam == StartParam && co.EndParam == EndParam;
        }

        // concrete methods

        public float ParamCoordinateDist(float p1, float p2)
        {
            return (Pos(p1) - Pos(p2)).magnitude;
        }

        private float ClampToParamRange(float p)
        {
            return Mathf.Min(Mathf.Max(p, StartParam), EndParam);
        }

        protected abstract Vector2 ComputePos_Inner(float param);

        internal bool Adjoins(Curve c2, float tol)
        {
            return StartPos.Equals(c2.EndPos, tol) || EndPos.Equals(c2.StartPos, tol);
        }
    }
}

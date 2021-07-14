using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public abstract class Curve : EqualityBase
    {
        public readonly float StartParam;
        public readonly float EndParam;
        public readonly int Id;
        private static int IdCounter = 0;

        protected Curve(float start_param, float end_param)
        {
            StartParam = start_param;
            EndParam = end_param;
            Id = IdCounter++;

            if (EndParam - StartParam < 1e-12)
                throw new NotSupportedException("StartParam must be < EndParam");
        }

        // exquisite abstractions

        protected abstract Vector2 ComputePos_Inner(float param);

        public float? FindParamForPoint(Vector2 pnt, float tol)
        {
            float? ret = FindParamForPoint_Inner(pnt, tol);

            Assertion.Assert(!ret.HasValue || (ComputePos(ret.Value) - pnt).magnitude < tol);

            return ret;
        }

        protected abstract float? FindParamForPoint_Inner(Vector2 pnt, float tol);

        public abstract Curve CloneWithChangedParams(float start, float end);

        public abstract Area BoundingArea();

        public abstract Vector2 Tangent(float param);

        public abstract Curve Merge(Curve c_after);

        public abstract float Length();

        public abstract Vector2 ComputeNormal(float p);

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

        public Vector2 StartPos()
        {
            return ComputePos(StartParam);
        }

        public Vector2 EndPos()
        {
            return ComputePos(EndParam);
        }

        public float ParamRange()
        {
            return EndParam - StartParam;
        }

        public float ParamCoordinateDist(float p1, float p2)
        {
            return (ComputePos(p1) - ComputePos(p2)).magnitude;
        }

        public Vector2 ComputePos(float p)
        {
            p = ClampToParamRange(p);

            return ComputePos_Inner(p);
        }

        private float ClampToParamRange(float p)
        {
            return Mathf.Min(Mathf.Max(p, StartParam), EndParam);
        }
    }
}

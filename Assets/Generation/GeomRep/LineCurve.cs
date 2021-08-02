using Assets.Extensions;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    [System.Diagnostics.DebuggerDisplay("From = {StartPos}, To = {EndPos}")]
    public class LineCurve : Curve
    {
        public readonly Vector2 Position;
        public readonly Vector2 Direction;

        public static LineCurve MakeFromPoints(Vector2 from, Vector2 to)
        {
            Vector2 vec = (to - from);
            return new LineCurve(from, vec.normalized, vec.magnitude);
        }

        public LineCurve(Vector2 position, Vector2 directionCosines, float length)
            : base(0, length)
        {
            Position = position;
            Direction = directionCosines;

            if (!Direction.IsUnit(1.0e-6f))
            {
                throw new ArgumentException();
            }
        }

        public LineCurve(Vector2 position, Vector2 directionCosines, float start, float end)
            : base(start, end)
        {

            Position = position;
            Direction = directionCosines;
        }

        protected override Vector2 ComputePos_Inner(float param)
        {
            return Position + Direction * param;
        }

        protected override float FindParamForPoint_Inner(Vector2 pnt)
        {
            Vector2 relative = pnt - Position;

            //if (Mathf.Abs(relative.Dot(Direction.Rot90())) > tol)
            //    return null;

            float par = relative.Dot(Direction);

            return par;
        }

        public override Curve CloneWithChangedParams(float start, float end)
        {
            return new LineCurve(Position, Direction, start, end);
        }

        public override Box2 BoundingArea
        {
            get => new Box2(StartPos.Min(EndPos), StartPos.Max(EndPos));
        }

        public override Vector2 Tangent(float param)
        {
            return Direction;
        }

        public override Curve Merge(Curve c_after)
        {
            if (c_after == this)
            {
                return null;
            }

            if (!(c_after is LineCurve))
            {
                return null;
            }

            LineCurve c_lc = (LineCurve)c_after;

            // could also look if they merge the other way around, but current usage knows
            // the expected order, so no need yet...
            if ((EndPos - c_lc.StartPos).sqrMagnitude > 1e-6f)
            {
                return null;
            }

            if (Direction != c_lc.Direction)
            {
                return null;
            }

            return new LineCurve(Position, Direction, StartParam, EndParam + c_lc.ParamRange);
        }

        public override float Length
        {
            get => EndParam - StartParam;
        }

        public override Vector2 Normal(float v)
        {
            return Direction.Rot270();
        }

        public override int GetHashCode()
        {
            // all that matters for line identity is the start and end pos
            return StartPos.GetHashCode() * 31 ^ EndPos.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(o, this))
            {
                return true;
            }

            if (!(o is LineCurve))
            {
                return false;
            }

            LineCurve lc_o = (LineCurve)o;

            // the lines are equal if their begining and end are equal, even if the
            // params and pos used to achieve that differ
            return StartPos == lc_o.StartPos && EndPos == lc_o.EndPos;
        }
    }
}

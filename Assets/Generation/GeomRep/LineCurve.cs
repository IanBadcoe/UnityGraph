using Assets.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class LineCurve : Curve
    {
        public readonly Vector2 Position;
        public readonly Vector2 Direction;

        public LineCurve(Vector2 position, Vector2 directionCosines, float length)
            : base (0, length)
        {

            if (position == null)
                throw new NullReferenceException("'position' cannot be null");

            if (directionCosines == null)
                throw new NullReferenceException("'directionCosines' cannot be null");

            Position = position;
            Direction = directionCosines;

            if (!Direction.IsUnit(1.0e-6f))
                throw new ArgumentException();
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

        protected override float? FindParamForPoint_Inner(Vector2 pnt, float tol)
        {
            Vector2 relative = pnt - Position;

            if (Mathf.Abs(relative.Dot(Direction.Rot90())) > tol)
                return null;

            float par = relative.Dot(Direction);

            if (!WithinParams(par, tol))
                return null;

            return par;
        }

        public override Curve CloneWithChangedParams(float start, float end)
        {
            return new LineCurve(Position, Direction, start, end);
        }

        public override Area BoundingArea()
        {
            return new Area(StartPos().Min(EndPos()), StartPos().Max(EndPos()));
        }

        public override Vector2 Tangent(float param)
        {
            return Direction;
        }

        public override Curve Merge(Curve c_after)
        {
            if (c_after == this)
                return null;

            if (!(c_after is LineCurve))
                return null;

            LineCurve c_lc = (LineCurve)c_after;
            // could loop for coaxial line swith different origins here
            // but current use is more to re-merge stuff we temporarily split
            // and that all leaves Position the same in both halves
            if (Position != c_lc.Position)
                return null;

            if (Direction != c_lc.Direction)
                return null;

            if (EndParam != c_lc.StartParam)
                return null;

            return new LineCurve(Position, Direction, StartParam, c_lc.EndParam);
        }

        public override float Length()
        {
            return EndParam - StartParam;
        }

        public override Vector2 ComputeNormal(float v)
        {
            return Direction.Rot270();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode_Inner() * 17 + Position.GetHashCode() * 31 ^ Direction.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(o, this))
                return true;

            if (!(o is LineCurve))
                return false;

            if (!base.Equals_Inner(o))
                return false;

            LineCurve lc_o = (LineCurve)o;

            return Position == lc_o.Position && Direction == lc_o.Direction;
        }
    }
}

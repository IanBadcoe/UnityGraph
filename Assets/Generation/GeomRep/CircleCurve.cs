using Assets.Generation.U;
using System;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    [System.Diagnostics.DebuggerDisplay("Pos = {Position}, From = {StartPos}, To = {EndPos}, Dir = {Rotation}")]
    public class CircleCurve : Curve
    {
        public enum RotationDirection
        {
            Forwards,
            Reverse
        }

        readonly public Vector2 Position;
        readonly public float Radius;
        readonly public RotationDirection Rotation;

        public CircleCurve(Vector2 position, float radius)
            : this(position, radius, 0, Mathf.PI * 2)
        {
        }

        public CircleCurve(Vector2 position, float radius,
                           float start_angle, float end_angle)
            : this(position, radius, start_angle, end_angle, RotationDirection.Forwards)
        {
        }

        public CircleCurve(Vector2 position, float radius,
                           RotationDirection rotation)
            : this(position, radius, 0, Mathf.PI * 2, rotation)
        {
        }

        private static float FixEndAngle(float start_angle, float end_angle)
        {
            start_angle = Util.FixupAngle(start_angle);
            end_angle = Util.FixupAngle(end_angle);

            if (end_angle <= start_angle)
            {
                end_angle += Mathf.PI * 2;
            }

            return end_angle;
        }

        public CircleCurve(Vector2 position, float radius,
                           float start_angle, float end_angle,
                           RotationDirection rotation)
            : base(Util.FixupAngle(start_angle), FixEndAngle(start_angle, end_angle))
        {

            Position = position;
            Radius = radius;
            Rotation = rotation;

            if (Position == null)
            {
                throw new NullReferenceException("null position");
            }

            if (Radius <= 0)
            {
                throw new ArgumentException("-ve or zero radius");
            }
        }

        protected override Vector2 ComputePos_Inner(float param)
        {
            if (Rotation == RotationDirection.Forwards)
            {
                return Position + new Vector2(Mathf.Sin(param), Mathf.Cos(param)) * Radius;
            }

            return Position + new Vector2(Mathf.Sin(-param), Mathf.Cos(-param)) * Radius;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode_Inner() * 17
                  ^ Position.GetHashCode() * 31
                  ^ Radius.GetHashCode() * 11
                  ^ (Rotation == RotationDirection.Forwards ? 1 : 0);
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(o, this))
            {
                return true;
            }

            if (!(o is CircleCurve))
            {
                return false;
            }

            if (!base.Equals_Inner(o))
            {
                return false;
            }

            CircleCurve cc_o = (CircleCurve)o;

            return Position.Equals(cc_o.Position)
                  && Radius == cc_o.Radius
                  && Rotation == cc_o.Rotation;
        }

        protected override float FindParamForPoint_Inner(Vector2 pnt)
        {
            Vector2 relative = pnt - Position;

            // in all known usages we _know_ the point is logically on the curve...
            //if (Mathf.Abs(relative.magnitude - Radius) > tol)
            //    return null;

            float ang = Util.Atan2(relative);

            if (Rotation == RotationDirection.Reverse)
            {
                ang = -ang;
            }

            // atan2 returns between -pi and + pi
            // we use 0 -> 2pi
            // BUT, we also require EndParam > StartParam
            while (ang < StartParam)
            {
                ang += 2 * Mathf.PI;
            }

            return ang;
        }

        public override Curve CloneWithChangedParams(float start, float end)
        {
            return new CircleCurve(Position, Radius, start, end, Rotation);
        }

        public override Area BoundingArea
        {
            // use whole circle here as the use I have for the moment doesn't need anything
            // tighter
            //
            // full solution is to union together startPos, EndPos and whichever of
            // 0, pi/2, pi and 3pi/2 points are within param range
            get => new Area(Position - new Vector2(Radius, Radius),
                Position + new Vector2(Radius, Radius));
        }

        public override Vector2 Tangent(float param)
        {
            if (Rotation == RotationDirection.Reverse)
            {
                return new Vector2(-Mathf.Cos(-param), Mathf.Sin(-param));
            }

            return new Vector2(Mathf.Cos(param), -Mathf.Sin(param));
        }

        public override Curve Merge(Curve c_after)
        {
            if (c_after == this)
            {
                return null;
            }

            if (!(c_after is CircleCurve))
            {
                return null;
            }

            CircleCurve c_cc = (CircleCurve)c_after;

            if (!Position.Equals(c_cc.Position))
            {
                return null;
            }

            if (Rotation != c_cc.Rotation)
            {
                return null;
            }

            if (Radius != c_cc.Radius)
            {
                return null;
            }

            if (!Util.ClockAwareAngleCompare(EndParam, c_cc.StartParam, 1e-12f))
            {
                return null;
            }

            return new CircleCurve(Position, Radius, StartParam, c_cc.EndParam, Rotation);
        }

        public override float Length
        {
            get => Radius * (EndParam - StartParam);
        }

        public override Vector2 Normal(float p)
        {
            Vector2 normal = new Vector2(Mathf.Sin(p), Mathf.Cos(p));

            if (Rotation == RotationDirection.Reverse)
            {
                normal = -normal;
            }

            return normal;
        }

        public override bool WithinParams(float p, float tol)
        {
            if (IsCyclic())
            {
                return true;
            }

            // we've fixed start param to lie between 0 and 2pi
            // and end param to be < 2pi above that
            // so if we are below start param and we step up one full turn
            // that either takes us right past end param (because we were too high)
            // or it takes us past it because we were too low and shouldn't have stepped up
            // or it leaves us below end param in which case we are in range
            if (p < StartParam)
            {
                p += Mathf.PI * 2;
            }

            return p < EndParam + tol;
        }

        public bool IsCyclic()
        {
            return Util.ClockAwareAngleCompare(StartParam, EndParam, 1e-6f);
        }
    }
}

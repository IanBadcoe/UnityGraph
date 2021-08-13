using Assets.Generation.U;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public enum RotationDirection
    {
        Forwards,
        Reverse,
        DontCare
    }

    public static class RotationDirectionExtensions
    {
        public static RotationDirection Invert(this RotationDirection rot)
        {
            switch (rot)
            {
                case RotationDirection.Forwards:
                    return RotationDirection.Reverse;
                case RotationDirection.Reverse:
                    return RotationDirection.Forwards;
                default:
                    return RotationDirection.DontCare;
            }
        }
    }

    [System.Diagnostics.DebuggerDisplay("Pos = {Position}, From = {StartPos}, To = {EndPos}, Dir = {Rotation}")]
    public class CircleCurve : Curve
    {
        public override float StartParam { get => AngleRange.Start; }
        public override float EndParam { get => AngleRange.End; }

        readonly public AngleRange AngleRange;
        readonly public Vector2 Position;
        readonly public float Radius;
        readonly public RotationDirection Rotation;

        public bool IsCyclic { get => AngleRange.IsCyclic; }

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

        public CircleCurve(Vector2 position, float radius,
                           float start_angle, float end_angle,
                           RotationDirection rotation)
            : this(position, radius, new AngleRange(start_angle, end_angle), rotation)
        {
        }

        public CircleCurve(Vector2 position, float radius,
                           AngleRange angle_range,
                           RotationDirection rotation)
        {
            AngleRange = angle_range;

            // we have to have a direction
            Assertion.Assert(rotation != RotationDirection.DontCare);

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

        public override bool Equals(Curve c, float tol)
        {
            if (ReferenceEquals(c, this))
            {
                return true;
            }

            if (!(c is CircleCurve))
            {
                return false;
            }

            CircleCurve cc = (CircleCurve)c;

            if (!AngleRange.Equals(cc.AngleRange, tol))
            {
                return false;
            }

            return Position.Equals(cc.Position)
                  && Radius == cc.Radius
                  && Rotation == cc.Rotation;
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

        public override Box2 BoundingArea
        {
            // use whole circle here as the use I have for the moment doesn't need anything
            // tighter
            //
            // full solution is to union together startPos, EndPos and whichever of
            // 0, pi/2, pi and 3pi/2 points are within param range
            get => new Box2(Position - new Vector2(Radius, Radius),
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

            if (!Util.ClockAwareAngleCompare(EndParam, c_cc.StartParam, 1e-5f))
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
            if (IsCyclic)
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

        public override Curve Reversed()
        {
            // start and end remain the same way around for a reversed circle
            // we just flip the "Rotation" field to say we mean the other direction
            return new CircleCurve(Position, Radius, StartParam, EndParam,
                Rotation.Invert());
        }

        public override Tuple<IList<Curve>, IList<Curve>> SplitCoincidentCurves(Curve c2, float tol)
        {
            if (!(c2 is CircleCurve))
            {
                return null;
            }

            var cc2 = c2 as CircleCurve;

            if ((Position - cc2.Position).magnitude > tol)
            {
                return null;
            }

            if (Mathf.Abs(Radius - cc2.Radius) > tol)
            {
                return null;
            }

            var common_range = AngleRange.ClockAwareRangeOverlap(cc2.AngleRange, tol);

            if (common_range == null)
            {
                return null;
            }

            IList<Curve> ret1 = new List<Curve> { this };
            IList<Curve> ret2 = new List<Curve> { c2 };

            foreach (var r in common_range)
            {
                ConditionalSplitCurveList(tol, ret1, r.Start);
                ConditionalSplitCurveList(tol, ret1, r.End);
                ConditionalSplitCurveList(tol, ret2, r.Start);
                ConditionalSplitCurveList(tol, ret2, r.End);
            }

            // if there was no split, we have only the original curve in the output,
            // and no need to return that...
            if (ret1.Count == 1)
            {
                ret1 = null;
            }

            if (ret2.Count == 1)
            {
                ret2 = null;
            }

            if (ret1 == null && ret2 == null)
            {
                return null;
            }

            return new Tuple<IList<Curve>, IList<Curve>>(ret1, ret2);
        }

        private static void ConditionalSplitCurveList(float tol, IList<Curve> curve_list, float split_param)
        {
            for (int i = 0; i < curve_list.Count; i++)
            {
                Curve c = curve_list[i];

                // we have to be on the same rotation around the clock
                split_param = AngleRange.FixupAngleRelative(c.StartParam, split_param);

                // negative tolerance requires us to be significantly within, e.g. not just on the endpoint
                // "WithinParams is not suitable here, because what we really mean in this case is
                // whether we are significantly away from an existing end
                // and full circles have everything "within params" but still have theoretical ends
                // which we do not need to split if we hit them...
                //if (c.WithinParams(split_param, -tol))
                if (split_param > c.StartParam + tol && split_param < c.EndParam - tol)
                {
                    curve_list[i] = c.CloneWithChangedParams(c.StartParam, split_param);
                    curve_list.Insert(i + 1, c.CloneWithChangedParams(split_param, c.EndParam));

                    // we really ought to hit only one curve with one split-point
                    return;
                }
            }
        }

        public override bool SameSupercurve(Curve curve, float tol)
        {
            CircleCurve cc = curve as CircleCurve;

            if (cc == null)
            {
                return false;
            }

            return Mathf.Abs(Radius - cc.Radius) < tol
                && (Position - cc.Position).magnitude < tol;
        }
    }
}

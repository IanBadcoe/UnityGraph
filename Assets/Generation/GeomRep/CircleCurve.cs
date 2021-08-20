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
            : this(position, radius, new AngleRange(start_angle, end_angle),
                  new AngleRange(start_angle, end_angle).Direction)
        {
        }

        public CircleCurve(Vector2 position, float radius,
                           RotationDirection rotation)
            : this(position, radius, new AngleRange(rotation), rotation)
        {
        }

        public CircleCurve(Vector2 position, float radius,
                           float start_angle, float end_angle,
                           RotationDirection rotation)
            : this(position, radius, new AngleRange(start_angle, end_angle), rotation)
        {
        }

        public CircleCurve(Vector2 position, float radius,
                   AngleRange angle_range)
            : this(position, radius, angle_range, angle_range.Direction)
        {
        }

        public CircleCurve(Vector2 position, float radius,
                           AngleRange angle_range,
                           RotationDirection rotation)
            : base(0, 1)
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

            if (rotation != angle_range.Direction && angle_range.Direction != RotationDirection.DontCare)
            {
                throw new ArgumentException("Inconsistent circle direction");
            }

            if (Mathf.Abs(AngleRange.Range) > Mathf.PI * 2) {
                throw new ArgumentException("More than a full turn in a circle");
            }
        }

        public float ParamToAngle(float param)
        {
            return AngleRange.Range * param + AngleRange.Start;
        }

        // just does the scaling, doesn't worry if we're within range
        public float AngleToParam(float angle)
        {
            if (AngleRange.Range != 0)
            {
                return (angle - AngleRange.Start) / AngleRange.Range;
            }

            // if the range is zero, all we can say is are we at the one point or not...
            if (Mathf.Abs(angle - AngleRange.Start) < 1e-4f)
            {
                return 0;
            }

            return -1;
        }

        protected override Vector2 ComputePos_Inner(float param)
        {
            float angle = ParamToAngle(param);

            return Position + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * Radius;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode_Inner() * 17
                  ^ AngleRange.GetHashCode() * 93
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

            ang = AngleRange.FixAngleForRange(ang);

            return AngleToParam(ang);
        }

        public override Curve CloneWithChangedExtents(float start, float end)
        {
            return new CircleCurve(Position, Radius, ParamToAngle(start), ParamToAngle(end), Rotation);
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
            float angle = ParamToAngle(param);

            var tang = new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle));

            if (Rotation == RotationDirection.Reverse)
            {
                tang = -tang;
            }

            return tang;
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

            if (Util.ClockAwareAngleCompare(AngleRange.End, c_cc.AngleRange.Start, 1e-5f))
            {
                return new CircleCurve(Position, Radius, AngleRange.Start, AngleRange.End + c_cc.AngleRange.Range, Rotation);
            }

            if (Util.ClockAwareAngleCompare(AngleRange.Start, c_cc.AngleRange.End, 1e-5f))
            {
                return new CircleCurve(Position, Radius, c_cc.AngleRange.Start, c_cc.AngleRange.End + AngleRange.Range, Rotation);
            }

            return null;
        }

        public override float Length
        {
            get => Radius * AngleRange.Length;
        }

        public override Vector2 Normal(float param)
        {
            float angle = ParamToAngle(param);

            Vector2 normal = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

            if (Rotation == RotationDirection.Reverse)
            {
                normal = -normal;
            }

            return normal;
        }

        public override bool WithinParams(float p, float tol)
        {
            // a bit ugh, as out of range values are not really "within params"
            // but they are "interprettable as a param" since a full circle can seamlessly
            // run off either end
            //
            // maybe add an "allow_cyclic" flag (and ignore in LineCurve)?
            if (IsCyclic)
            {
                return true;
            }

            return p > StartParam - tol && p < EndParam + tol;
        }

        public override Curve Reversed()
        {
            return new CircleCurve(Position, Radius, AngleRange.Reversed());
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

        private static void ConditionalSplitCurveList(float tol, IList<Curve> curve_list, float split_angle)
        {
            for (int i = 0; i < curve_list.Count; i++)
            {
                CircleCurve c = curve_list[i] as CircleCurve;

                // we have to be on the same rotation around the clock
                split_angle = c.AngleRange.FixAngleForRange(split_angle);

                // negative tolerance requires us to be significantly within, e.g. not just on the endpoint
                // and ignore cyclic, because what we are looking for here is not hitting the break,
                // rather than just falling in range
                if (c.AngleRange.InRange(split_angle, false, -tol))
                {
                    float split_param = c.AngleToParam(split_angle);
                    curve_list[i] = c.CloneWithChangedExtents(0, split_param);
                    curve_list.Insert(i + 1, c.CloneWithChangedExtents(split_param, 1));

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

using Assets.Extensions;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    [System.Diagnostics.DebuggerDisplay("From = {StartPos}, To = {EndPos}")]
    public class LineCurve : Curve
    {
        public override float StartParam { get; }
        public override float EndParam { get; }

        public readonly Vector2 Position;
        public readonly Vector2 Direction;

        public static LineCurve MakeFromPoints(Vector2 from, Vector2 to)
        {
            Vector2 vec = (to - from);
            return new LineCurve(from, vec.normalized, vec.magnitude);
        }

        public static Curve MakeFromPoints(int v1, int v2, int v3, int v4)
        {
            return MakeFromPoints(new Vector2(v1, v2), new Vector2(v3, v4));
        }

        public static Loop MakeRect(float x1, float y1, float x2, float y2)
        {
            Assertion.Assert(x1 <= x2);
            Assertion.Assert(y1 <= y2);

            var c1 = new Vector2(x1, y1);
            var c2 = new Vector2(x1, y2);
            var c3 = new Vector2(x2, y2);
            var c4 = new Vector2(x2, y1);

            return new Loop(
                new List<Curve> {
                    MakeFromPoints(c1, c2),
                    MakeFromPoints(c2, c3),
                    MakeFromPoints(c3, c4),
                    MakeFromPoints(c4, c1),
                });
        }

        public LineCurve(Vector2 position, Vector2 directionCosines, float length)
            : this(position, directionCosines, 0, length)
        {
        }

        public LineCurve(Vector2 position, Vector2 directionCosines, float start, float end)
        {
            StartParam = start;
            EndParam = end;
            Position = position;
            Direction = directionCosines;

            if (!Direction.IsUnit(1.0e-6f))
            {
                throw new ArgumentException();
            }
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

        public override bool Equals(Curve c, float tol)
        {
            if (ReferenceEquals(c, this))
            {
                return true;
            }

            if (!(c is LineCurve))
            {
                return false;
            }

            LineCurve lc = (LineCurve)c;

            // the lines are equal if their begining and end are equal, even if the
            // params and pos used to achieve that differ
            return StartPos == lc.StartPos && EndPos == lc.EndPos;
        }

        public override Curve Reversed()
        {
            return LineCurve.MakeFromPoints(EndPos, StartPos);
        }

        public float Slope()
        {
            return (EndPos.y - StartPos.y) / (EndPos.x - StartPos.x);
        }

        public override Tuple<IList<Curve>, IList<Curve>> SplitCoincidentCurves(Curve c2, float tol)
        {
            // we can never split ourselves
            if (Equals(c2, tol))
            {
                return null;
            }

            if (!(c2 is LineCurve))
            {
                return null;
            }

            LineCurve lc2 = c2 as LineCurve;

            if (!SameSupercurve(lc2, tol))
            {
                return null;
            }

            float c1_c_start = FindParamForPoint_Clamped(lc2.StartPos, tol);
            float c1_c_end = FindParamForPoint_Clamped(lc2.EndPos, tol);
            float c2_c_start = lc2.FindParamForPoint_Clamped(StartPos, tol);
            float c2_c_end = lc2.FindParamForPoint_Clamped(EndPos, tol);

            float c1_c_lower = Math.Min(c1_c_start, c1_c_end);
            float c1_c_higher = Math.Max(c1_c_start, c1_c_end);
            float c2_c_lower = Math.Min(c2_c_start, c2_c_end);
            float c2_c_higher = Math.Max(c2_c_start, c2_c_end);

            IList<Curve> ret1 = new List<Curve> { this };
            IList<Curve> ret2 = new List<Curve> { c2 };

            ConditionalSplitCurveList(tol, ret1, c1_c_lower);
            ConditionalSplitCurveList(tol, ret1, c1_c_higher);

            ConditionalSplitCurveList(tol, ret2, c2_c_lower);
            ConditionalSplitCurveList(tol, ret2, c2_c_higher);

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

                // negative tolerance requires us to be significantly within, e.g. not just on the endpoint
                // "WithinParams is not suitable here, because what we really mean in this case is
                // whether we are significantly away from an existing end
                if (split_param > c.StartParam + tol && split_param < c.EndParam - tol)
                {
                    curve_list[i] = c.CloneWithChangedParams(c.StartParam, split_param);
                    curve_list.Insert(i + 1, c.CloneWithChangedParams(split_param, c.EndParam));

                    // we really ought to hit only one curve with one split-point
                    return;
                }
            }
        }

        [System.Diagnostics.DebuggerDisplay("Normal = {Normal}, Dist = {Dist}")]
        public class NormalAndDistLineParams : EqualityBase
        {
            public readonly Vector2 Normal;
            public readonly float Dist;

            public NormalAndDistLineParams(Vector2 normal, float dist)
            {
                Normal = normal;
                Dist = dist;
            }

            public static NormalAndDistLineParams operator -(NormalAndDistLineParams val)
            {
                return new NormalAndDistLineParams(-val.Normal, -val.Dist);
            }

            private bool Equals_Internal(NormalAndDistLineParams other, float tol)
            {
                if (Mathf.Abs(other.Dist - Dist) > tol)
                {
                    return false;
                }

                return (other.Normal - Normal).magnitude <= tol;
            }

            public bool Equals(NormalAndDistLineParams other, float tol)
            {
                var neg = -this;

                return Equals_Internal(other, tol)
                    || neg.Equals_Internal(other, tol);
            }

            public override bool Equals(object o)
            {
                if (!(o is NormalAndDistLineParams))
                {
                    return false;
                }

                var other = o as NormalAndDistLineParams;

                return Equals(other, 0.0f);
            }

            public override int GetHashCode()
            {
                return Normal.GetHashCode() ^ Dist.GetHashCode() * 7;
            }
        }

        // Returns the params for an implicit equation:
        // p . normal - dist = 0
        public NormalAndDistLineParams GetNormAndDistDescription()
        {
            Vector2 norm = Normal(0);
            float dist = StartPos.Dot(norm);

            // wanted to settle on one of the two equivalent solutions
            // (norm, dist) vs. (-norm, -dist) but numerical precision problems
            // mean we cannot reliably tell them apart (the same line reversed can still give
            // different solutions if dist is v.v.close to zero)

            return new NormalAndDistLineParams(norm, dist);
        }

        public override bool SameSupercurve(Curve curve, float tol)
        {
            LineCurve lc = curve as LineCurve;

            if (lc == null)
            {
                return false;
            }

            // this returns a normalised return, so the direction and dist will be the same
            // even if the lines run in opposite directions
            var desc = GetNormAndDistDescription();
            var lc_desc = lc.GetNormAndDistDescription();

            return desc.Equals(lc_desc, tol);
        }
    }
}

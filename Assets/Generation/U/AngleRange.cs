using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.U
{
    public class AngleRange : EqualityBase
    {
        public readonly float Start;
        public readonly float End;

        public AngleRange(float start, float end)
        {
            Start = FixupAngle(start);
            End = FixupEndAngle(start, end);
        }

        public bool IsCyclic
        {
            get => Util.ClockAwareAngleCompare(Start, End, 1e-6f);
        }

        public bool Equals(AngleRange other, float tol)
        {
            return Mathf.Abs(Start - other.Start) <= tol
                && Mathf.Abs(End - other.End) <= tol;
        }

        public override bool Equals(object o)
        {
            if (!(o is AngleRange))
            {
                return false;
            }

            return Equals(o as AngleRange, 0.0f);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ 3 * End.GetHashCode();
        }

        // removes any positive or negative whole turns to leave a number
        // between 0.0 and 2 PI
        public static float FixupAngle(float a)
        {
            while (a < 0)
            {
                a += Mathf.PI * 2;
            }

            while (a >= Mathf.PI * 2)
            {
                a -= Mathf.PI * 2;
            }

            return a;
        }

        // find a value for the second angle that is 0 < angle <= 2 * PI 
        //
        // does not assume that the start angle is already in the correct range, and
        // fixes that up first
        //
        // (end angles have the special property that they can be > PI * 2 in order to make them
        //  greater than the start angle)
        public static float FixupEndAngle(float start_angle, float end_angle)
        {
            start_angle = FixupAngle(start_angle);
            end_angle = FixupAngle(end_angle);

            while (end_angle <= start_angle)
            {
                end_angle += Mathf.PI * 2;
            }

            return end_angle;
        }

        // the difference between this and the previous is this makes no assumptions about what range
        // start_angle should be in
        //
        // and that this, given two identical inputs assumes that a difference of zero is intended,
        // not 2 * PI
        public static float FixupAngleRelative(float relative_to, float relative_angle)
        {
            relative_angle = FixupAngle(relative_angle);

            while (relative_angle < relative_to)
            {
                relative_angle += Mathf.PI * 2;
            }

            return relative_angle;
        }

        public IList<AngleRange> ClockAwareRangeOverlap(AngleRange b, float tol)
        {
            IList<AngleRange> ret = new List<AngleRange>();

            // special cases, if either range is a whole circle, then the return is just the other range
            // (awkward to work that out with the below code...)
            if (IsCyclic)
            {
                ret.Add(b);

                return ret;
            }

            if (b.IsCyclic)
            {
                ret.Add(this);

                return ret;
            }

            {
                // rotate angles by whole turns so that b_start is > a_start
                // (both ends are already > their resp. start)
                float b_start_r = FixupAngleRelative(Start, b.Start);
                // shift b.End by the same amount
                float b_end_r = b.End + b_start_r - b.Start;

                if (b_start_r + tol < End)
                {
                    ret.Add(new AngleRange(
                        b_start_r, Math.Min(End, b_end_r)));
                }
            }

            {
                // rotate angles by whole turns so that both ends are >= their starts,
                // and a_start is > b_start
                float a_start_r = FixupAngleRelative(b.Start, Start);
                float a_end_r = End + a_start_r - Start;

                if (a_start_r + tol < b.End)
                {
                    ret.Add(new AngleRange(
                        a_start_r, Math.Min(b.End, a_end_r)));
                }
            }

            if (ret.Count == 0)
            {
                return null;
            }

            // two ranges with the same start can trigger both the above clauses
            // but we only have one overlap
            if (ret.Count == 2 && ret[0] == ret[1])
            {
                ret.RemoveAt(1);
            }

            return ret;
        }
    }
}

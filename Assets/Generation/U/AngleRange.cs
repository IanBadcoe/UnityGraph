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

            return FixupAngleRelative(start_angle, end_angle);
        }

        // the difference between this and the previous is this makes no assumptions about what range
        // start_angle should be in
        private static float FixupAngleRelative(float start_angle, float end_angle)
        {
            end_angle = FixupAngle(end_angle);

            while (end_angle <= start_angle)
            {
                end_angle += Mathf.PI * 2;
            }

            return end_angle;
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
                // rotate angles by whole turns so that both ends are >= their starts,
                // and b_start is > a_start
                float a_end_r = FixupAngleRelative(Start, End);
                float b_start_r = FixupAngleRelative(Start, b.Start);
                float b_end_r = FixupAngleRelative(b_start_r, b.End);

                if (b_start_r + tol < a_end_r)
                {
                    ret.Add(new AngleRange(
                        b_start_r, Math.Min(a_end_r, b_end_r)));
                }
            }

            {
                // rotate angles by whole turns so that both ends are >= their starts,
                // and a_start is > b_start
                float b_end_r = FixupAngleRelative(b.Start, b.End);
                float a_start_r = FixupAngleRelative(b.Start, Start);
                float a_end_r = FixupAngleRelative(a_start_r, End);

                if (a_start_r + tol < b_end_r)
                {
                    ret.Add(new AngleRange(
                        a_start_r, Math.Min(b_end_r, a_end_r)));
                }
            }

            if (ret.Count == 0)
            {
                return null;
            }

            return ret;
        }
    }
}

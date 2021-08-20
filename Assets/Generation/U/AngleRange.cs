using Assets.Generation.GeomRep;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.U
{
    [System.Diagnostics.DebuggerDisplay("Start = {Start}, End = {End}")]
    public class AngleRange : EqualityBase
    {
        public readonly float Start;
        public readonly float End;
        // goes -ve for reverse ranges
        public float Range { get => End - Start; }
        // hard to find good names here, but "Range" is signed, "Length" is not
        public float Length { get => Math.Abs(Range); }
        public float Min { get => Math.Min(Start, End); }
        public float Max { get => Math.Max(Start, End); }

        public RotationDirection Direction
        {
            get
            {
                if (Start < End)
                {
                    return RotationDirection.Forwards;
                } else if (Start > End)
                {
                    return RotationDirection.Reverse;
                }

                return RotationDirection.DontCare;
            }
        }

        // make an empty range
        public AngleRange()
        {
            Start = 0;
            End = 0;
        }

        public AngleRange(RotationDirection rotation)
        {
            if (rotation == RotationDirection.Reverse)
            {
                Start = Mathf.PI * 2;
                End = 0;
            }
            else
            {
                // if rotation is "don't care", either would be fine, so take forwards
                Start = 0;
                End = Mathf.PI * 2;
            }
        }

        public AngleRange(float start, float end)
        {
            Start = start;
            End = end;

            // angle summation can leave us minutely over
            if (Length > Mathf.PI * 2 + 1e-5f)
            {
                throw new ArgumentException("More than a full turn in AngleRange");
            }

            switch(Direction)
            {
                case RotationDirection.Forwards:
                    Start = FixupAngle(Start);
                    End = FixupEndAngle(Start, End);
                    break;
                case RotationDirection.Reverse:
                    End = FixupAngle(End);
                    Start = FixupEndAngle(End, Start);
                    break;
                case RotationDirection.DontCare:
                    Start = End = FixupAngle(Start);
                    break;
            }
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
            var a = AsForwardsRange();
            b = b.AsForwardsRange();

            IList<AngleRange> ret = new List<AngleRange>();

            // special cases, if either range is a whole circle, then the return is just the other range
            // (awkward to work that out with the below code...)
            //
            // EXCEPT (warning: awkward matter of definitions) if the two ranges are:
            // 0 -> 2PI and PI -> 3PI, then returning either single representation can be misleading
            // (e.g. when we use this for splitting curves at their "conincident portions" we actually need the end
            //  of the overlap projected onto each curve, and for this case those are 0 and PI,
            //  so here we allow adding both, but if they are the same one will get removed before we
            //  return.  And if we leave both in, that will give us both splitpoints)

            if (a.IsCyclic)
            {
                ret.Add(b);
            }

            if (b.IsCyclic)
            {
                ret.Add(a);
            }

            if (!a.IsCyclic && !b.IsCyclic)
            {

                {
                    // rotate angles by whole turns so that b_start is >= a_start
                    // (both ends are already > their resp. start)
                    float b_start_r = FixupAngleRelative(a.Start, b.Start);
                    // shift b.End by the same amount
                    float b_end_r = b.End + b_start_r - b.Start;

                    if (b_start_r + tol < a.End)
                    {
                        ret.Add(new AngleRange(
                            b_start_r, Math.Min(a.End, b_end_r)));
                    }
                }

                {
                    // rotate angles by whole turns so that both ends are >= their starts,
                    // and a_start is > b_start
                    float a_start_r = FixupAngleRelative(b.Start, a.Start);
                    float a_end_r = a.End + a_start_r - a.Start;

                    if (a_start_r + tol < b.End)
                    {
                        ret.Add(new AngleRange(
                            a_start_r, Math.Min(b.End, a_end_r)));
                    }
                }
            }

            if (ret.Count == 0)
            {
                return null;
            }

            // two ranges with the same start can trigger both the above clauses
            // but we only have one overlap
            if (ret.Count == 2 && ret[0].Equals(ret[1], tol))
            {
                ret.RemoveAt(1);
            }

            return ret;
        }

        private AngleRange AsForwardsRange()
        {
            if (Direction == RotationDirection.Reverse)
            {
                return Reversed();
            }

            return this;
        }

        public float FixAngleForRange(float ang, float tol = 0)
        {
            float min = Math.Min(Start, End);

            // step down until we're definitely below our range, then step up again until we are just above,
            // which either puts us in the range or off the other end
            while (ang >= min - tol)
            {
                ang -= Mathf.PI * 2;
            }

            while (ang < min - tol)
            {
                ang += Mathf.PI * 2;
            }

            return ang;
        }

        public bool InRange(float angle, bool consider_cyclic = true, float tol = 1e-5f)
        {
            if (consider_cyclic)
            {
                // anything is in range of a full rotation
                if (IsCyclic)
                    return true;

                // otherwise bring us round to the same rotation
                angle = FixAngleForRange(angle, tol);
            }

            return angle >= Min - tol && angle <= Max + tol;
        }

        public AngleRange Reversed()
        {
            return new AngleRange(End, Start);
        }
    }
}

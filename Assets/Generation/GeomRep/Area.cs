using Assets.Extensions;
using Assets.Generation.U;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class Area : EqualityBase
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public Vector2 Diagonal { get => Max - Min; }

        static public Area Empty { get; } = new Area();

        public Area()
        {
            Min = new Vector2(0, 0);
            Max = new Vector2(-1, -1);
        }

        public Area(Vector2 bl, Vector2 tr)
        {
            Min = bl;
            Max = tr;
        }

        public bool IsEmpty()
        {
            // if either high edge is on the wrong side of the low edge, we are empty
            // (empty is different from zero-sized, edges are in the same place for that)
            return Min.x > Max.x || Min.y > Max.y;
        }

        public Area Union(Area rhs)
        {
            // empty areas add nothing to other areas
            if (IsEmpty())
            {
                return rhs;
            }
            else if (rhs.IsEmpty())
            {
                return this;
            }

            return new Area(Min.Min(rhs.Min), Max.Max(rhs.Max));
        }

        public bool Disjoint(Area rhs)
        {
            return Min.x > rhs.Max.x
                || Min.y > rhs.Max.y
                || Max.x < rhs.Min.x
                || Max.y < rhs.Min.y;
        }

        public override int GetHashCode()
        {
            if (IsEmpty())
            {
                return 0x48e5083f;
            }

            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj) || obj.GetType() != GetType())
            {
                return false;
            }

            Area a_obj = obj as Area;

            return Min == a_obj.Min && Max == a_obj.Max;
        }

        internal Vector3 Centre()
        {
            return (Min + Max) / 2;
        }
    }
}

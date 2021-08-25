using Assets.Extensions;
using Assets.Generation.U;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    [System.Diagnostics.DebuggerDisplay("Min = {Min}, Max = {Max}, Diag = {Diagonal} {IsEmpty ? \"Empty\" : \"\"}")]
    public class Box2 : EqualityBase
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public Vector2 Diagonal { get => Max - Min; }

        static public Box2 Empty { get; } = new Box2();

        public Box2()
        {
            Min = new Vector2(0, 0);
            Max = new Vector2(-1, -1);
        }

        public Box2(Vector2 bl, Vector2 tr)
        {
            Min = bl;
            Max = tr;
        }

        public Box2(float x1, float y1, float x2, float y2)
            : this(new Vector2(x1, y1), new Vector2(x2, y2))
        {
        }

        public bool IsEmpty
        {
            // if either high edge is on the wrong side of the low edge, we are empty
            // (empty is different from zero-sized, edges are in the same place for that)
            get => Min.x > Max.x || Min.y > Max.y;
        }

        public Box2 Union(Box2 rhs)
        {
            // empty areas add nothing to other areas
            if (IsEmpty)
            {
                return rhs;
            }
            else if (rhs.IsEmpty)
            {
                return this;
            }

            return new Box2(Min.Min(rhs.Min), Max.Max(rhs.Max));
        }

        public Box2 Union(Vector2 rhs)
        {
            // empty areas add nothing to other areas
            if (IsEmpty)
            {
                return new Box2(rhs, rhs);
            }

            return new Box2(Min.Min(rhs), Max.Max(rhs));
        }

        public bool Disjoint(Box2 rhs)
        {
            return Min.x > rhs.Max.x
                || Min.y > rhs.Max.y
                || Max.x < rhs.Min.x
                || Max.y < rhs.Min.y;
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
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

            Box2 a_obj = obj as Box2;

            return Min == a_obj.Min && Max == a_obj.Max;
        }

        public Vector3 Centre()
        {
            return (Min + Max) / 2;
        }
    }
}

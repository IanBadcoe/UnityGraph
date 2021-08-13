using UnityEngine;

namespace Assets.Extensions
{
    public static class Vector2Extensions
    {
        public static float Dot(this Vector2 lhs, Vector2 rhs)
        {
            return lhs.x * rhs.x
                 + lhs.y * rhs.y;
        }

        // OK, so cross product doesn't exist for 2D vectors, but this is
        // for angle signs and is effectively promoting the 2D vectors to 3D,
        // taking the cross product and returning the only component which can be non-zero
        // e.g. Z
        public static float Cross(this Vector2 lhs, Vector2 rhs)
        {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }

        public static Vector2 Max(this Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
        }

        public static Vector2 Min(this Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
        }

        public static Vector2 Rot90(this Vector2 lhs)
        {
            return new Vector2(lhs.y, -lhs.x);
        }

        public static Vector2 Rot270(this Vector2 lhs)
        {
            return new Vector2(-lhs.y, lhs.x);
        }

        public static bool IsUnit(this Vector2 lhs)
        {
            return lhs.sqrMagnitude == 1.0f;
        }

        public static bool IsUnit(this Vector2 lhs, float tolerance)
        {
            return Mathf.Abs(lhs.magnitude - 1) < tolerance;
        }

        public static bool Equals(this Vector2 lhs, Vector2 rhs, float tolerance)
        {
            return (lhs - rhs).sqrMagnitude < tolerance * tolerance;
        }
    }
}
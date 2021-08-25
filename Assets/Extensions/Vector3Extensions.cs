using UnityEngine;

namespace Assets.Extensions
{
    public static class Vector3Extensions
    {
        public static bool Equals(this Vector3 lhs, Vector3 rhs, float tolerance)
        {
            return (lhs - rhs).sqrMagnitude < tolerance * tolerance;
        }
    }
}
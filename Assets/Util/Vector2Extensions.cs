using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Util
{
    static class Vector2Extensions
    {
        public static float Dot(this Vector2 lhs, Vector2 rhs) {
            return lhs.x * rhs.x
                 + lhs.y * rhs.y;
        }
    }
}

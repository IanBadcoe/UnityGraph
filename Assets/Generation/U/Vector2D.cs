using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.U
{
    class Vector2D
    {
        public readonly double X;
        public readonly double Y;

        public Vector2D()
        {
            X = Y = 0;
        }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double SqrMagnitude {
            get => X * X + Y * Y;
        }

        public static Vector2D operator-(Vector2D lhs, Vector2D rhs)
        {
            return new Vector2D(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }
    }
}

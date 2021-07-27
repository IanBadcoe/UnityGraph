using System;

namespace Assets.Generation.U
{
    public class Vector2D
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

        public double Magnitude
        {
            get => Math.Sqrt(SqrMagnitude);
        }

        public double SqrMagnitude
        {
            get => X * X + Y * Y;
        }

        public double Dot(Vector2D rhs)
        {
            return X * rhs.X + Y * rhs.Y;
        }

        public static Vector2D operator -(Vector2D lhs, Vector2D rhs)
        {
            return new Vector2D(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vector2D operator +(Vector2D lhs, Vector2D rhs)
        {
            return new Vector2D(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vector2D operator /(Vector2D lhs, double rhs)
        {
            return new Vector2D(lhs.X / rhs, lhs.Y / rhs);
        }

        public static Vector2D operator *(Vector2D lhs, double rhs)
        {
            return new Vector2D(lhs.X * rhs, lhs.Y * rhs);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class CircularGeomLayout : GeomLayout
    {
        public readonly Vector2 Position;
        public readonly float Radius;

        public CircularGeomLayout(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public override Loop MakeBaseGeometry()
        {
            return new Loop(new CircleCurve(Position, Radius));
        }

        public override LoopSet MakeDetailGeometry()
        {
            return null;
        }
    }
}

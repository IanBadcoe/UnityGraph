using Assets.Generation.G;
using Assets.Generation.G.GLInterfaces;
using System;
using UnityEngine;

namespace Assets.Generation.GeomRep.Layouts
{
    class CircularPillarLayoutFactory : IGeomLayoutFactory
    {
        public IGeomLayout Create(INode n)
        {
            return new CircularPillarGeomLayout(n.Position, n.Radius);
        }

        public IGeomLayout Create(DirectedEdge de)
        {
            throw new NotImplementedException();
        }
    }

    public class CircularPillarGeomLayout : GeomLayout
    {
        private readonly Vector2 m_position;
        private readonly float m_rad;

        public CircularPillarGeomLayout(Vector2 position, float radius)
        {
            m_position = position;
            m_rad = radius;
        }

        public override Loop MakeBaseGeometry()
        {
            return new Loop(new CircleCurve(m_position, m_rad));
        }

        public override LoopSet MakeDetailGeometry()
        {
            LoopSet ret = new LoopSet();
            ret.Add(new Loop(new CircleCurve(m_position, m_rad / 2, CircleCurve.RotationDirection.Reverse)));

            return ret;
        }
    }
}

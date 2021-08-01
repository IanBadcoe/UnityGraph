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
            return new CircularPillarGeomLayout();
        }

        public IGeomLayout Create(DirectedEdge de)
        {
            throw new NotImplementedException();
        }
    }

    public class CircularPillarGeomLayout : GeomLayout
    {
        public override Loop MakeBaseGeometry(INode node)
        {
            return new Loop(new CircleCurve(node.Position, node.Radius));
        }

        public override LoopSet MakeDetailGeometry(INode node)
        {
            LoopSet ret = new LoopSet();
            ret.Add(new Loop(new CircleCurve(node.Position, node.Radius / 2, CircleCurve.RotationDirection.Reverse)));

            return ret;
        }
    }

    class FourCircularPillarsLayoutFactory : IGeomLayoutFactory
    {
        public IGeomLayout Create(INode n)
        {
            return new FourCircularPillarsGeomLayout();
        }

        public IGeomLayout Create(DirectedEdge de)
        {
            throw new NotImplementedException();
        }
    }

    public class FourCircularPillarsGeomLayout : GeomLayout
    {
        public override Loop MakeBaseGeometry(INode node)
        {
            return new Loop(new CircleCurve(node.Position, node.Radius));
        }

        public override LoopSet MakeDetailGeometry(INode node)
        {
            LoopSet ret = new LoopSet();

            for (int i = 0; i < 4; i++)
            {
                float ang = Mathf.PI * 2 * i / 4;
                Vector2 pos = node.Position + new Vector2(Mathf.Sin(ang) * node.Radius / 2, Mathf.Cos(ang) * node.Radius / 2);

                ret.Add(new Loop(new CircleCurve(pos, node.Radius / 6, CircleCurve.RotationDirection.Reverse)));
            }

            return ret;
        }
    }
}

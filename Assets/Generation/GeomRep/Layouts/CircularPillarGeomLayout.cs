﻿using Assets.Generation.G;
using UnityEngine;

namespace Assets.Generation.GeomRep.Layouts
{
    public class CircularPillarGeomLayout : GeomLayout
    {
        public static GeomLayout Instance { get; } = new CircularPillarGeomLayout();

        private CircularPillarGeomLayout() { }

        public override Loop MakeBaseGeometry(INode node)
        {
            return new Loop(new CircleCurve(node.Position, node.Radius));
        }

        public override LoopSet MakeDetailGeometry(INode node)
        {
            LoopSet ret = new LoopSet
            {
                new Loop(new CircleCurve(node.Position, node.Radius / 2, RotationDirection.Reverse))
            };

            return ret;
        }
    }

    public class FourCircularPillarsGeomLayout : GeomLayout
    {
        public static GeomLayout Instance { get; } = new FourCircularPillarsGeomLayout();

        private FourCircularPillarsGeomLayout() { }

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

                ret.Add(new Loop(new CircleCurve(pos, node.Radius / 6, RotationDirection.Reverse)));
            }

            return ret;
        }
    }
}

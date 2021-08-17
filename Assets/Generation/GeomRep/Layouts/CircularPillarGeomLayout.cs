using Assets.Generation.G;
using UnityEngine;

namespace Assets.Generation.GeomRep.Layouts
{
    public class CircularPillarGeomLayout : GeomLayout
    {
        public static GeomLayout Instance { get; } = new CircularPillarGeomLayout();

        private CircularPillarGeomLayout() { }

        public override LoopSet MakeGeometry(Node node)
        {
            if (node.WallThickness > 0)
            {
                return new LoopSet {
                    new Loop("wall", new CircleCurve(node.Position, node.Radius)),
                    new Loop("floor", new CircleCurve(node.Position, node.Radius - node.WallThickness)),
                    new Loop("pillar", new CircleCurve(node.Position, (node.Radius - node.WallThickness) / 2))
                };
            }

            return new LoopSet {
                new Loop("floor", new CircleCurve(node.Position, node.Radius)),
                new Loop("pillar", new CircleCurve(node.Position, node.Radius / 2))
            };
        }
    }

    public class FourCircularPillarsGeomLayout : GeomLayout
    {
        public static GeomLayout Instance { get; } = new FourCircularPillarsGeomLayout();

        private FourCircularPillarsGeomLayout() { }

        public override LoopSet MakeGeometry(Node node)
        {
            LoopSet ret = new LoopSet();

            float effective_radius = node.Radius;

            if (node.WallThickness > 0)
            {
                effective_radius -= node.WallThickness;
                ret.Add(new Loop("wall", new CircleCurve(node.Position, node.Radius)));
            }

            ret.Add(new Loop("floor", new CircleCurve(node.Position, effective_radius)));
            ret.Add(new Loop("water", new CircleCurve(node.Position, effective_radius / 2)));

            for (int i = 0; i < 4; i++)
            {
                float ang = Mathf.PI * 2 * i / 4;
                Vector2 pos = node.Position + new Vector2(Mathf.Sin(ang) * effective_radius / 2, Mathf.Cos(ang) * effective_radius / 2);

                ret.Add(new Loop("decor", new CircleCurve(pos, effective_radius / 6)));
            }

            return ret;
        }
    }
}

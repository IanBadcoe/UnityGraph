using Assets.Generation.G;
using UnityEngine;

namespace Assets.Generation.GeomRep.Layouts
{
    public class CircularPillarGeomLayout : GeomLayout
    {
        public static GeomLayout Instance { get; } = new CircularPillarGeomLayout();

        private CircularPillarGeomLayout() { }

        public override LoopSet MakeGeometry(INode node)
        {
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

        public override LoopSet MakeGeometry(INode node)
        {
            LoopSet ret = new LoopSet();
            ret.Add(new Loop("floor", new CircleCurve(node.Position, node.Radius)));

            for (int i = 0; i < 4; i++)
            {
                float ang = Mathf.PI * 2 * i / 4;
                Vector2 pos = node.Position + new Vector2(Mathf.Sin(ang) * node.Radius / 2, Mathf.Cos(ang) * node.Radius / 2);

                ret.Add(new Loop("decor", new CircleCurve(pos, node.Radius / 6)));
            }

            return ret;
        }
    }
}

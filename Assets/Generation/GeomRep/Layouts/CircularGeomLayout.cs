using Assets.Generation.G;

namespace Assets.Generation.GeomRep
{
    public class CircularGeomLayout : GeomLayout
    {
        static public GeomLayout Instance { get; } = new CircularGeomLayout();

        private CircularGeomLayout() { }

        public override LoopSet MakeGeometry(INode node)
        {
            return new LoopSet {
                new Loop("floor", new CircleCurve(node.Position, node.Radius))
            };
        }
    }
}

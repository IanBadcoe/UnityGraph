using Assets.Generation.G;

namespace Assets.Generation.GeomRep
{
    public class CircularGeomLayout : GeomLayout
    {
        static public GeomLayout Instance { get; } = new CircularGeomLayout();

        private CircularGeomLayout() { }

        public override Loop MakeBaseGeometry(INode node)
        {
            return new Loop(new CircleCurve(node.Position, node.Radius));
        }
    }
}

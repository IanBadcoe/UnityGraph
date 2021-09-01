using Assets.Generation.G;

namespace Assets.Generation.GeomRep
{
    public class CircularGeomLayout : GeomLayout
    {
        static public GeomLayout Instance { get; } = new CircularGeomLayout();

        private CircularGeomLayout() { }

        public override LoopSet MakeGeometry(Node node)
        {
            if (node.Radius == 0)
            {
                return null;
            }

            if (node.WallThickness > 0)
            {
                return new LoopSet {
                    new Loop("wall", new CircleCurve(node.Position, node.Radius)),
                    new Loop("floor", new CircleCurve(node.Position, node.Radius - node.WallThickness))
                };
            }

            return new LoopSet {
                new Loop("floor", new CircleCurve(node.Position, node.Radius))
            };
        }
    }

    public class CircularFireLakeGeomLayout : GeomLayout
    {
        static public GeomLayout Instance { get; } = new CircularFireLakeGeomLayout();

        private CircularFireLakeGeomLayout() { }

        public override LoopSet MakeGeometry(Node node)
        {
            float floor_radius = node.Radius - node.WallThickness;

            if (node.WallThickness > 0)
            {
                return new LoopSet {
                    new Loop("wall", new CircleCurve(node.Position, node.Radius)),
                    new Loop("floor", new CircleCurve(node.Position, floor_radius)),
                    new Loop("fire", new CircleCurve(node.Position, floor_radius * 0.75f)),
                    new Loop("fire", new CircleCurve(node.Position, floor_radius * 0.25f, RotationDirection.Reverse))
                };
            }

            return new LoopSet {
                new Loop("floor", new CircleCurve(node.Position, node.Radius)),
                new Loop("fire", new CircleCurve(node.Position, floor_radius * 0.75f)),
                new Loop("fire", new CircleCurve(node.Position, floor_radius * 0.25f, RotationDirection.Reverse))
            };
        }
    }
}

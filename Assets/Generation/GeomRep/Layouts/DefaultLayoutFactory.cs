using Assets.Generation.G;
using Assets.Generation.G.GLInterfaces;

namespace Assets.Generation.GeomRep
{
    class DefaultLayoutFactory : IGeomLayoutFactory
    {
        public IGeomLayout Create(INode n)
        {
            return new CircularGeomLayout(n.Position, n.Radius);
        }

        public IGeomLayout Create(DirectedEdge de)
        {
            return MakeDefaultCorridor(de);
        }

        public static GeomLayout MakeDefaultCorridor(DirectedEdge de)
        {
            // scale the corridor rectangle's width down slightly
            // so that it doesn't precisely hit at a tangent to any adjoining junction-node's circle
            // -- that causes awkward numerical precision problems in the curve-curve intersection routines
            // which can throw out the union operation
            return new RectangularGeomLayout(de.Start.Position,
                  de.End.Position, de.HalfWidth * 0.99f);
        }

    }
}

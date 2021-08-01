using Assets.Generation.G;
using Assets.Generation.G.GLInterfaces;
using Assets.Generation.U;

namespace Assets.Generation.GeomRep
{
    abstract public class GeomLayout : IGeomLayout
    {
        // one +ve loop that cuts the outer envelope of the space the node or edge will occupy
        public virtual Loop MakeBaseGeometry(INode node)
        {
            return null;
        }
        public virtual Loop MakeBaseGeometry(DirectedEdge edge)
        {
            return null;
        }

        // one or more -ve loops that put things like pillars back inside
        // the base geometry
        public virtual LoopSet MakeDetailGeometry(INode node)
        {
            return null;
        }
        public virtual LoopSet MakeDetailGeometry(DirectedEdge edge)
        {
            return null;
        }
    }
}

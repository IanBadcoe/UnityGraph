using Assets.Generation.G;

namespace Assets.Generation.GeomRep
{
    abstract public class GeomLayout
    {
        // one +ve loop that cuts the outer envelope of the space the node or edge will occupy
        public virtual LoopSet MakeGeometry(INode node)
        {
            return null;
        }
        public virtual LoopSet MakeGeometry(DirectedEdge edge)
        {
            return null;
        }
    }
}

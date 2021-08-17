using Assets.Generation.G;

namespace Assets.Generation.GeomRep
{
    abstract public class GeomLayout
    {
        public virtual LoopSet MakeGeometry(Node node)
        {
            return null;
        }
        public virtual LoopSet MakeGeometry(DirectedEdge edge)
        {
            return null;
        }
    }
}

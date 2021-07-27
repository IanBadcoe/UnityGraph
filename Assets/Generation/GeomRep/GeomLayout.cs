using Assets.Generation.G.GLInterfaces;

namespace Assets.Generation.GeomRep
{
    abstract public class GeomLayout : IGeomLayout
    {
        // one +ve loop that cuts the outer envelope of the space the node will occupy
        public abstract Loop MakeBaseGeometry();

        // one or more -ve loops that put things like pillars back inside
        // the base geometry
        public abstract LoopSet MakeDetailGeometry();
    }
}

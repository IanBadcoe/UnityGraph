namespace Assets.Generation.G.GLInterfaces
{
    // blind interface so graph doesn't need any geometry implementation details
    public interface IGeomLayout
    {

    }

    public interface IGeomLayoutFactory
    {
        IGeomLayout Create(INode n);
        IGeomLayout Create(DirectedEdge de);
    }
}
using Assets.Generation.U;

namespace Assets.Generation.G
{
    public class DirectedEdgePair : EqualityBase
    {
        public readonly DirectedEdge Edge1;
        public readonly DirectedEdge Edge2;

        public DirectedEdgePair(DirectedEdge e1, DirectedEdge e2)
        {
            Edge1 = e1;
            Edge2 = e2;
        }

        public override int GetHashCode()
        {
            int x = Edge1.GetHashCode();
            int y = Edge2.GetHashCode();

            // we want this symmetric as in this case which edge is which is irrelevant
            return x ^ y;
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            DirectedEdgePair dep = o as DirectedEdgePair;

            return (Edge1 == dep.Edge1 && Edge2 == dep.Edge2)
                || (Edge1 == dep.Edge2 && Edge2 == dep.Edge1);
        }
    }
}

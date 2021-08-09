namespace Assets.Generation.G
{
    [System.Diagnostics.DebuggerDisplay("({T1}, {T2})")]
    public class IntersectionResult : DirectedEdgePair
    {
        public readonly float T1;
        public readonly float T2;

        public IntersectionResult(DirectedEdge e1, DirectedEdge e2, float t1, float t2)
            : base(e1, e2)
        {
            T1 = t1;
            T2 = t2;
        }

        // hashing and equality not overridden as identity comes from which edges, not where the
        // intersection was...
    }
}

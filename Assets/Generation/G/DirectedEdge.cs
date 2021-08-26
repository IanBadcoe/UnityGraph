using Assets.Generation.GeomRep;
using Assets.Generation.U;

namespace Assets.Generation.G
{
    [System.Diagnostics.DebuggerDisplay("Start = {Start}, End = {End}, Min = {MinLength}, Max = {MaxLength}")]
    public class DirectedEdge : EqualityBase
    {
        public readonly Node Start;
        public readonly Node End;
        public readonly float MinLength;
        public readonly float MaxLength;
        public readonly float HalfWidth;
        public readonly float WallThickness;

        public readonly GeomLayout Layout;

        public static DirectedEdge Exemplar = new DirectedEdge();

        private DirectedEdge()
        {
            // default corridor, only for use as "Exemplar" above
            // no-one else is allowed null Start/End
            MaxLength = 1;
            MinLength = 1;
            HalfWidth = 1;
            WallThickness = 0.1f;
            Layout = CorridorLayout.Default;
        }

        // for identity/searching purposes, only start and end count...
        public DirectedEdge(Node start, Node end)
            : this(start, end, 0, 0, 0)
        {
        }

        public DirectedEdge(Node start, Node end,
            float min_length, float max_length,
            float half_width, float wall_thickness = 0,
            GeomLayout layout = null)
        {
            Assertion.Assert(start != null);
            Assertion.Assert(end != null);

            Start = start;
            End = end;
            MaxLength = max_length;
            MinLength = min_length;
            HalfWidth = half_width;
            WallThickness = wall_thickness;
            Layout = layout;
        }

        public override int GetHashCode()
        {
            int x = Start.GetHashCode();
            int y = End.GetHashCode();

            // we don't intend to ever have two edges between the same
            // pair of nodes, so no need to look at lengths
            return x * 31 + y;
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            DirectedEdge e = o as DirectedEdge;

            return Start == e.Start && End == e.End;
        }

        public Node OtherNode(Node n)
        {
            if (n == Start)
            {
                return End;
            }
            else if (n == End)
            {
                return Start;
            }

            return null;
        }

        public float Length()
        {
            return (End.Position - Start.Position).magnitude;
        }

        public bool Connects(Node n)
        {
            return n == Start || n == End;
        }
    }
}
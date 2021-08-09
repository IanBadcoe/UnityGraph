using Assets.Generation.GeomRep;
using Assets.Generation.U;

namespace Assets.Generation.G
{
    [System.Diagnostics.DebuggerDisplay("Start = {Start}, End = {End}, Min = {MinLength}, Max = {MaxLength}")]
    public class DirectedEdge : EqualityBase
    {
        public readonly INode Start;
        public readonly INode End;
        public readonly float MinLength;
        public readonly float MaxLength;
        public readonly float HalfWidth;

        public readonly GeomLayout Layout;

        public DirectedEdge(INode start, INode end,
              float min_length, float max_length,
              float half_width)
            : this(start, end, min_length, max_length, half_width, null)
        {
        }

        public DirectedEdge(INode start, INode end,
              float min_length, float max_length,
              float half_width,
              GeomLayout layout)
        {
            Assertion.Assert(start != null);
            Assertion.Assert(end != null);

            Start = start;
            End = end;
            MinLength = min_length;
            MaxLength = max_length;
            HalfWidth = half_width;
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

        public INode OtherNode(INode n)
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

        public bool Connects(INode n)
        {
            return n == Start || n == End;
        }

        public object Colour { get; internal set; } = 0xff4b4b4b;
    }
}
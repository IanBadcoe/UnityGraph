using Assets.Generation.GeomRep;

namespace Assets.Generation.Templates
{
    [System.Diagnostics.DebuggerDisplay("Name = {Name}, Type = {Type}, Codes = {Codes}")]
    public sealed class NodeRecord
    {
        public enum NodeType
        {
            In,
            Out,
            Internal,
            Target
        }

        public readonly NodeType Type;
        public readonly string Name;
        public readonly bool Nudge;
        public readonly NodeRecord PositionOn;       // required
        public readonly NodeRecord PositionTowards;  // null for none
        public readonly NodeRecord PositionAwayFrom; // null for none
        public readonly string Codes;                // copied onto node
        public readonly float Radius;
        public readonly GeomLayout Layout;

        public NodeRecord(NodeType type, string name,
              bool nudge, NodeRecord positionOn, NodeRecord positionTowards, NodeRecord positionAwayFrom,
              string codes, float radius,
              GeomLayout layout)
        {
            Type = type;
            Name = name;
            Nudge = nudge;
            PositionOn = positionOn;
            PositionTowards = positionTowards;
            PositionAwayFrom = positionAwayFrom;
            Codes = codes;
            Radius = radius;
            Layout = layout;
        }
    }
}

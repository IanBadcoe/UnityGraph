using Assets.Generation.G.GLInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.Templates
{
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
        public readonly uint Colour;
        public readonly IGeomLayoutFactory LayoutCreator;

        public NodeRecord(NodeType type, string name,
              bool nudge, NodeRecord positionOn, NodeRecord positionTowards, NodeRecord positionAwayFrom,
              string codes, float radius, uint colour,
              IGeomLayoutFactory layoutCreator)
        {
            Type = type;
            Name = name;
            Nudge = nudge;
            PositionOn = positionOn;
            PositionTowards = positionTowards;
            PositionAwayFrom = positionAwayFrom;
            Codes = codes;
            Radius = radius;
            Colour = colour;
            LayoutCreator = layoutCreator;
        }
    }
}

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
        public readonly String Name;
        public readonly bool Nudge;
        public readonly NodeRecord PositionOn;       // required
        public readonly NodeRecord PositionTowards;  // null for none
        public readonly NodeRecord PositionAwayFrom; // null for none
        public readonly String Codes;                // copied onto node
        public readonly float Radius;
        public readonly int Colour;
        //public readonly GeomLayout.IGeomLayoutCreateFromNode GeomCreator;

        NodeRecord(NodeType type, String name,
              bool nudge, NodeRecord positionOn, NodeRecord positionTowards, NodeRecord positionAwayFrom,
              String codes, float radius, int colour/*,
              GeomLayout.IGeomLayoutCreateFromNode geomCreator*/)
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
            // GeomCreator = geomCreator;
        }
    }

}

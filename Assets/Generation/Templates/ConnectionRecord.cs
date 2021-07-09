using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.Templates
{
    public sealed class ConnectionRecord
    {
        public readonly NodeRecord From;
        public readonly NodeRecord To;
        public readonly float MinLength;
        public readonly float MaxLength;
        public readonly float HalfWidth;
        public readonly int Colour;

        ConnectionRecord(NodeRecord from, NodeRecord to,
                         float min_length, float max_length,
                         float half_width,
                         int colour)
        {
            From = from;
            To = to;
            MinLength = min_length;
            MaxLength = max_length;
            HalfWidth = half_width;
            Colour = colour;
        }
    }
}

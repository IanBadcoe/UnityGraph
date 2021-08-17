using Assets.Generation.GeomRep;

namespace Assets.Generation.Templates
{
    [System.Diagnostics.DebuggerDisplay("From = {From}, To = {To}, Min = {MinLength}, Max = {MaxLength}")]
    public sealed class ConnectionRecord
    {
        public readonly NodeRecord From;
        public readonly NodeRecord To;
        public readonly float MaxLength;
        public readonly float HalfWidth;
        public readonly float WallThickness;
        public readonly GeomLayout Layout;

        public ConnectionRecord(NodeRecord from, NodeRecord to,
                         float max_length,
                         float half_width,
                         float wall_thickness,
                         GeomLayout layout)
        {
            From = from;
            To = to;
            MaxLength = max_length;
            HalfWidth = half_width;
            WallThickness = wall_thickness;
            Layout = layout;
        }
    }
}

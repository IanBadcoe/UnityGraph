using Assets.Extensions;
using Assets.Generation.G;
using Assets.Generation.U;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class CorridorLayout : GeomLayout
    {
        public static GeomLayout Instance { get; } = new CorridorLayout();

        private CorridorLayout() { }

        public override Loop MakeBaseGeometry(DirectedEdge edge)
        {
            Vector2 dir = edge.End.Position - edge.Start.Position;
            float length = dir.magnitude;
            dir = dir / length;

            Vector2 width_dir = dir.Rot270();
            // scale the corridor rectangle's width down slightly
            // so that it doesn't precisely hit at a tangent to any adjoining junction-node's circle
            // -- that causes awkward numerical precision problems in the curve-curve intersection routines
            // which can throw out the union operation
            float actual_half_width = edge.HalfWidth * 0.99f;
            Vector2 half_width = width_dir * actual_half_width;

            Vector2 start_left = edge.Start.Position + half_width;
            Vector2 start_right = edge.Start.Position - half_width;
            Vector2 end_left = edge.End.Position + half_width;
            Vector2 end_right = edge.End.Position - half_width;

            List<Curve> curves = new List<Curve>();
            curves.Add(new LineCurve(start_left, dir, length));
            curves.Add(new LineCurve(end_left, -width_dir, actual_half_width * 2));
            curves.Add(new LineCurve(end_right, -dir, length));
            curves.Add(new LineCurve(start_right, width_dir, actual_half_width * 2));

            Assertion.Assert(curves[0].EndPos.Equals(curves[1].StartPos, 1e-4f));
            Assertion.Assert(curves[1].EndPos.Equals(curves[2].StartPos, 1e-4f));
            Assertion.Assert(curves[2].EndPos.Equals(curves[3].StartPos, 1e-4f));
            Assertion.Assert(curves[3].EndPos.Equals(curves[0].StartPos, 1e-4f));

            return new Loop(curves);
        }
    }
}

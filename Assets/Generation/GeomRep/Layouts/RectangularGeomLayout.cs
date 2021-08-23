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

        protected CorridorLayout() { }

        public override LoopSet MakeGeometry(DirectedEdge edge)
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

            if (edge.WallThickness > 0)
            {
                List<Curve> wall_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, actual_half_width);
                List<Curve> floor_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, actual_half_width - edge.WallThickness);

                return new LoopSet {
                    new Loop("wall", wall_curves),
                    new Loop("floor", floor_curves)
                };
            }
            else
            {
                List<Curve> floor_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, actual_half_width);

                return new LoopSet {
                    new Loop("floor", floor_curves)
                };
            }
        }

        protected static List<Curve> MakeRect(Vector2 start, Vector2 end, Vector2 dir, float length, Vector2 width_dir, float actual_half_width)
        {
            Vector2 half_width = width_dir * actual_half_width;

            Vector2 start_left = start + half_width;
            Vector2 start_right = start - half_width;
            Vector2 end_left = end + half_width;
            Vector2 end_right = end - half_width;

            List<Curve> curves = new List<Curve>
            {
                new LineCurve(start_left, dir, length),
                new LineCurve(end_left, -width_dir, actual_half_width * 2),
                new LineCurve(end_right, -dir, length),
                new LineCurve(start_right, width_dir, actual_half_width * 2)
            };
            return curves;
        }
    }

    public class FireCorridorLayout : CorridorLayout
    {
        public new static GeomLayout Instance { get; } = new FireCorridorLayout();

        private FireCorridorLayout() { }

        public override LoopSet MakeGeometry(DirectedEdge edge)
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

            if (edge.WallThickness > 0)
            {
                float floor_half_width = actual_half_width - edge.WallThickness;

                List<Curve> wall_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, actual_half_width);
                List<Curve> floor_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, floor_half_width);
                List<Curve> fire_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, floor_half_width * 0.5f);

                return new LoopSet {
                    new Loop("wall", wall_curves),
                    new Loop("floor", floor_curves),
                    new Loop("fire", fire_curves)
                };
            }
            else
            {
                List<Curve> floor_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, actual_half_width);
                List<Curve> fire_curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, actual_half_width * 0.5f);

                return new LoopSet {
                    new Loop("floor", floor_curves),
                    new Loop("fire", fire_curves)
                };
            }
        }
    }
}

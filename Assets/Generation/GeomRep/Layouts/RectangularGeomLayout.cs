using Assets.Extensions;
using Assets.Generation.G;
using Assets.Generation.U;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class CorridorLayout : GeomLayout
    {
        public struct LayerData
        {
            public readonly string LayerName;
            public readonly float HalfWidth;

            public LayerData(string name, float half_width) : this()
            {
                LayerName = name;
                HalfWidth = half_width;
            }
        }

        LayerData[] Layers;
        static Dictionary<string, CorridorLayout> CustomLayouts = new Dictionary<string, CorridorLayout>();

        public static GeomLayout Default { get; } = new CorridorLayout();

        public static GeomLayout Custom(string name)
        {
            return CustomLayouts[name];
        }

        public static void RegisterCustom(string name, params LayerData[] layers)
        {
            CustomLayouts[name] = new CorridorLayout(layers);
        }

        protected CorridorLayout() { }

        private CorridorLayout(params LayerData[] layers)
        {
            Layers = layers;
        }

        public override LoopSet MakeGeometry(DirectedEdge edge)
        {
            if (edge.HalfWidth == 0)
            {
                return null;
            }

            Vector2 dir = edge.End.Position - edge.Start.Position;
            float length = dir.magnitude;
            dir = dir / length;

            Vector2 width_dir = dir.Rot270();

            if (Layers == null)
            {
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

            LoopSet ret = new LoopSet();

            foreach(var l in Layers)
            {
                var actual_half_width = l.HalfWidth * 0.99f;

                List<Curve> curves = MakeRect(edge.Start.Position, edge.End.Position, dir, length, width_dir, actual_half_width);

                ret.Add(new Loop(l.LayerName, curves));
                ret.Add(new Loop(l.LayerName, new CircleCurve(edge.Start.Position, l.HalfWidth)));
                ret.Add(new Loop(l.LayerName, new CircleCurve(edge.End.Position, l.HalfWidth)));
            }

            return ret;
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
}

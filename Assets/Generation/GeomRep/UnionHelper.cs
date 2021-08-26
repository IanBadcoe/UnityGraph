using Assets.Behaviour;
using Assets.Generation.G;
using Assets.Generation.U;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    [System.Diagnostics.DebuggerDisplay("Loops = {m_base_loops.Count}, Details = {m_detail_loop_sets.Count}, Merged = {m_merged_loops.Count}")]
    public class UnionHelper
    {
        private readonly List<Loop> m_loops = new List<Loop>();
        private readonly Dictionary<string, Intersector> m_merged_loop_sets = new Dictionary<string, Intersector>();

        private Box2 m_bounds;
        private Vector2 m_start_pos;

        private readonly Intersector m_intersector = new Intersector();

        // exposed for testing but there could be cases where client code wants to reach-in
        // and add some special piece of geometry
        public void AddLoops(LoopSet ls)
        {
            m_loops.AddRange(ls);
        }

        public IReadOnlyList<Loop> Loops
        {
            get => m_loops;
        }

        // returns true when all complete
        public void UnionAll(ClRand r, LayerConfigBehaviour lcb)
        {
            while (m_loops.Count > 0)
            {
                Loop l = m_loops[0];
                string layer = l.Layer;


                if (!m_merged_loop_sets.TryGetValue(layer, out Intersector merged_layer))
                {
                    m_merged_loop_sets[layer] = merged_layer = new Intersector(l);
                }
                else
                {

                    merged_layer.Union(l, 1e-5f, r, layer);

                    m_loops.RemoveAt(0);
                }            
            }

            foreach(var cut_desc in lcb.CutSequence)
            {
                if (m_merged_loop_sets.TryGetValue(cut_desc.CutBy, out Intersector cut_by)
                    && m_merged_loop_sets.TryGetValue(cut_desc.Cut, out Intersector cut))
                {
                    cut.Cut(cut_by, 1e-5f, r, cut_desc.Cut);
                }
            }
        }

        public void GenerateGeometry(Graph graph)
        {
            // process edges first, because we want any -ve features of nodes to be able to cut the ends of them
            // e.g. an island in a lake of fire should cut the rect for the fire river coming in from the edge
            // and processing that second achieves that
            foreach (DirectedEdge de in graph.GetAllEdges())
            {
                GeomLayout gl = de.Layout;

                LoopSet loops = gl.MakeGeometry(de);

                if (loops != null)
                {
                    AddLoops(loops);
                }
            }

            foreach (Node n in graph.GetAllNodes())
            {
                GeomLayout gl = n.Layout;

                LoopSet loops = gl.MakeGeometry(n);

                // can have node with no geometry...  at least in unit-tests
                if (loops != null)
                {
                    AddLoops(loops);
                }
            }

            Node start = graph.GetAllNodes().Where(
                  n => n.Name == "Start").FirstOrDefault();

            if (start != null)
            {
                m_start_pos = start.Position;
            }
        }

        private void CalculateBounds()
        {
            m_bounds = m_merged_loop_sets
                .Values
                .Select(l => l.Merged.GetBounds())
                .Aggregate(new Box2(), (a, b) => a.Union(b));
        }

        public IReadOnlyDictionary<string, LoopSet> MergedLoops
        {
            get => m_merged_loop_sets
                .Select(x => new KeyValuePair<string, LoopSet>(x.Key, x.Value.Merged))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}

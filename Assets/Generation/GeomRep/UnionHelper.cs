﻿using Assets.Generation.G;
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
        private Dictionary<string, LoopSet> m_merged_loop_sets = new Dictionary<string, LoopSet>();

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
        public bool UnionOne(ClRand r)
        {
            if (m_loops.Count > 0)
            {
                Loop l = m_loops[0];
                string layer = l.Layer;

                LoopSet merged_layer_loops;

                if (!m_merged_loop_sets.TryGetValue(layer, out merged_layer_loops))
                {
                    merged_layer_loops = new LoopSet();
                }

                m_merged_loop_sets[layer] = m_intersector.Union(merged_layer_loops, new LoopSet(l), 1e-5f, r, layer);

                Assertion.Assert(m_merged_loop_sets[layer] != null);

                m_loops.RemoveAt(0);

                return false;
            }

            return true;
        }

        public void GenerateGeometry(Graph graph)
        {
            foreach (INode n in graph.GetAllNodes())
            {
                GeomLayout gl = n.Layout;

                LoopSet loops = gl.MakeGeometry(n);

                // can have node with no geometry...  at least in unit-tests
                if (loops != null)
                {
                    AddLoops(loops);
                }
            }

            foreach (DirectedEdge de in graph.GetAllEdges())
            {
                GeomLayout gl = de.Layout;

                LoopSet loops = gl.MakeGeometry(de);

                if (loops != null)
                {
                    AddLoops(loops);
                }
            }

            INode start = graph.GetAllNodes().Where(
                  n => n.Name == "Start").FirstOrDefault();

            if (start != null)
            {
                m_start_pos = start.Position;
            }
        }

        private void CalculateBounds()
        {
            m_bounds = m_merged_loop_sets.Values.Select(l => l.GetBounds()).Aggregate(new Box2(), (a, b) => a.Union(b));
        }

        public IReadOnlyDictionary<string, LoopSet> MergedLoops
        {
            get => m_merged_loop_sets;
        }
    }
}

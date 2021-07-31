using Assets.Generation.G;
using Assets.Generation.U;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class UnionHelper
    {
        private readonly List<Loop> m_base_loops = new List<Loop>();
        private readonly List<LoopSet> m_detail_loop_sets = new List<LoopSet>();
        private LoopSet m_merged_loops = new LoopSet();

        private Box2 m_bounds;
        private Vector2 m_start_pos;

        private readonly Intersector m_intersector = new Intersector();

        // exposed for testing but there could be cases where client code wants to reach-in
        // and add some special piece of geometry
        public void AddBaseLoop(Loop l)
        {
            m_base_loops.Add(l);
        }

        // exposed for testing but there could be cases where client code wants to reach-in
        // and add some special piece of geometry
        public void AddDetailLoops(LoopSet ls)
        {
            m_detail_loop_sets.Add(ls);
        }

        public IReadOnlyList<Loop> BaseLoops
        {
            get => m_base_loops;
        }

        public IReadOnlyList<LoopSet> DetailLoopSets
        {
            get => m_detail_loop_sets;
        }

        // returns true when all complete
        public bool UnionOne(ClRand r)
        {
            if (m_base_loops.Count > 0)
            {
                Loop l = m_base_loops[0];
                LoopSet ls = new LoopSet(l);

                m_merged_loops = m_intersector.Union(m_merged_loops, ls, 1e-5f, r);

                Assertion.Assert(m_merged_loops != null);

                m_base_loops.RemoveAt(0);

                return false;
            }

            if (m_detail_loop_sets.Count > 0)
            {
                LoopSet ls = m_detail_loop_sets[0];

                m_merged_loops = m_intersector.Union(m_merged_loops, ls, 1e-6f, r);

                Assertion.Assert(m_merged_loops != null);

                m_detail_loop_sets.RemoveAt(0);

                return false;
            }

            return true;
        }

        public void GenerateGeometry(Graph graph)
        {
            foreach (INode n in graph.GetAllNodes())
            {
                GeomLayout gl = n.LayoutCreator.Create(n) as GeomLayout;

                Loop bg = gl.MakeBaseGeometry();

                // can have node with no geometry...  at least in unit-tests
                if (bg != null)
                {
                    AddBaseLoop(bg);
                }

                LoopSet details = gl.MakeDetailGeometry();

                // can definitely have no details
                if (details != null)
                {
                    AddDetailLoops(details);
                }
            }

            foreach (DirectedEdge de in graph.GetAllEdges())
            {
                GeomLayout gl = de.LayoutCreator.Create(de) as GeomLayout;

                Loop l = gl.MakeBaseGeometry();

                if (l != null)
                {
                    AddBaseLoop(l);
                }

                LoopSet details = gl.MakeDetailGeometry();

                // can definitely have no details
                if (details != null)
                {
                    AddDetailLoops(details);
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
            m_bounds = m_merged_loops.Select(l => l.GetBounds()).Aggregate(new Box2(), (a, b) => a.Union(b));
        }

        //public Level makeLevel(floatcell_size, floatwall_facet_length)
        //{
        //    calculateBounds();

        //    Level ret = new Level(cell_size, wall_facet_length, m_bounds, m_start_pos);

        //    for (Loop l : m_merged_loops)
        //    {
        //        List<Tuple<Vector2, Vector2>> loop_pnts = l.facetWithNormals(wall_facet_length);

        //        Tuple<Vector2, Vector2> prev = loop_pnts.get(loop_pnts.size() - 1);

        //        WallLoop wl = new WallLoop();

        //        Wall prev_w = null;

        //        for (Tuple<Vector2, Vector2> curr : loop_pnts)
        //        {
        //            // normal (in "Second") is from 1/2 way along the segment that starts at "prev"
        //            Wall w = new Wall(prev.First, curr.First, prev.Second);

        //            if (prev_w != null)
        //            {
        //                w.setPrev(prev_w);
        //                prev_w.setNext(w);
        //            }

        //            wl.add(w);

        //            prev_w = w;

        //            prev = curr;
        //        }

        //        prev_w.setNext(wl.get(0));
        //        wl.get(0).setPrev(prev_w);

        //        ret.addWallLoop(wl);
        //    }

        //    return ret;
        //}

        public IReadOnlyList<Loop> MergedLoops
        {
            get => m_merged_loops;
        }
    }
}

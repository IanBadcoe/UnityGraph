using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine;

namespace Assets.Generation.Gen
{
    internal class RelaxerStepper_CGFactory : IRelaxerFactory
    {
        public IStepper MakeRelaxer(IoCContainer ioc_container, Graph g, GeneratorConfig c)
        {
            return new RelaxerStepper_CG(ioc_container, g, c);
        }
    }

    public class RelaxerStepper_CG : IStepper
    {
        private readonly IoCContainer ioc_container;
        private readonly Graph m_graph;
        private readonly GeneratorConfig m_config;

        alglib.mincgstate opt_state;

        private List<INode> m_nodes;
        private List<DirectedEdge> m_edges;
        private double[] pars;

        // whichever is smaller out of the summed-radii and the
        // shortest path through the graph between two nodes
        // we use this as d0 in the node <-> node force function
        // because otherwise a large node can force its second-closest
        // neighbour (and further) so far away that the edge gets split
        // and then the new second-closest neighbour is in the same position
        private ShortestPathFinder m_node_dists;

        private int m_energy_count = 0;

        public RelaxerStepper_CG(IoCContainer ioc_container, Graph g, GeneratorConfig c)
        {
            this.ioc_container = ioc_container;
            this.m_graph = g;
            this.m_config = c;
        }

        private void SetUp()
        {
            m_nodes = m_graph.GetAllNodes();
            m_edges = m_graph.GetAllEdges();

            // these are shortest path lengths through the graph
            //
            // irrespective of node <-> node or node <-> edge forces, we don't want to be pushed further than this
            // so we shorten the distances of those so they don't stretch edges too far
            //
            // (node <-> node and node <-> edge forces have to be stronger than edge forces
            // as we rely on edges stretching (in other cases) to tell ue when we need to
            // lengthen an edge (inserting a corner)
            m_node_dists = new ShortestPathFinder();

            m_node_dists.FindPathLengths(m_graph, x => (x.MaxLength + x.MinLength) / 2);

            int num_pars = m_nodes.Count * 2;

            pars = new double[num_pars];

            int p_num = 0;

            foreach(var n in m_nodes)
            {
                pars[p_num + 0] = n.Position.x;
                pars[p_num + 1] = n.Position.y;

                p_num += 2;
            }

            alglib.mincgcreatef(pars, 1e-6, out opt_state);
            alglib.mincgsetcond(opt_state, 0.001, 0, 0.01, 0);

#if DEBUG
            alglib.mincgoptguardsmoothness(opt_state);
#endif
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            SetUp();

            m_energy_count = 0;

            alglib.mincgoptimize(opt_state, EnergyFunc, null, null);

            int p_num = 0;

            foreach (var n in m_nodes)
            {
                n.Position = new Vector2((float)pars[p_num + 0], (float)pars[p_num + 1]);

                p_num += 2;
            }

            return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                  null,
                  ""
                  //" move:" + maxd +
                  //" time step:" + step +
                  //" force:" + maxf +
                  //" max edge stretch:" + max_edge_stretch +
                  //" max edge squeeze: " + max_edge_squeeze +
                  //" max edge side squeeze: " + max_edge_side_squeeze +
                  //" max node squeeze: " + max_node_squeeze
                  );
        }

        void EnergyFunc(double[] pars, ref double func, object o)
        {
            m_energy_count++;

            func = 0;

            for (int i = 0; i < m_nodes.Count - 1; i++)
            {
                var n1 = m_nodes[i];
                Vector2D n1pos = new Vector2D(pars[i * 2], pars[i * 2 + 1]);

                for (int j = i + 1; j < m_nodes.Count; j++)
                {
                    var n2 = m_nodes[j];
                    Vector2D n2pos = new Vector2D(pars[j * 2], pars[j * 2 + 1]);

                    double dist2 = (n1pos - n2pos).SqrMagnitude;

                    func += NodeNodeEnergy(dist2, m_node_dists.GetDist(n1, n2));
                }
            }
        }

        static double NodeNodeEnergy(double d2, double d0)
        {
            // dividing by d0 scales this to the desired radius

            double ratio = d2 / (d0 * d0);

            // going to try an equation 1 / (1 + N.Ratio^P)
            // this gives a curve which is a sigmoid
            // the higher P, the steeper the rise
            // the value at 1 (e.g. at D0) is 1/N
            //
            // so, for example, with N = 1 and P = 16, we have a very steep rise
            // centred on d0 (e.g. = 0.5 at D0)
            //
            // or with N = 9 and P = 8 we have a somewhat shallower rise, which is down to 1.0
            // by the time we hit D0
            //
            // both of these are trying for my objective which is having little force outside
            // D0, e.g. as much a step as possible...

            const int N = 9;
            const int P = 8;

            return 1 / (1 + N * Math.Pow(ratio, P / 2)); 
        }
   }
}
using Assets.Generation.G;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEngine;

namespace Assets.Generation.Gen
{
    public interface IRelaxationParamSource
    {
        int GetParams(List<double> pars, int offset);
        int SetParams(double[] array, int offset);
    }

    public class RelaxerStepper_CG : IStepper
    {
        public Graph Graph { get; private set; }

        public int MaxIterationsPerStep = 10;

        private readonly GeneratorConfig m_config;
        alglib.mincgstate opt_state;

        private List<INode> m_nodes;
        private List<DirectedEdge> m_edges;
        private List<Force> m_extra_forces;

        private double[] m_pars;
        private readonly Dictionary<IRelaxationParamSource, int> m_source2pars_idx
            = new Dictionary<IRelaxationParamSource, int>();


        // whichever is smaller out of the summed-radii and the
        // shortest path through the graph between two nodes
        // we use this as d0 in the node <-> node force function
        // because otherwise a large node can force its second-closest
        // neighbour (and further) so far away that the edge gets split
        // and then the new second-closest neighbour is in the same position
        private ShortestPathFinder m_node_dists;

        private int m_energy_count = 0;
        private int m_iterations = 0;

        readonly float MoveTarget;

        public enum TerminationCondition
        {
            InfOrNanError,
            GradientVerificationError,
            FunctionLimitReached,
            ParameterStepLimitReached,
            GradientLimitReached,
            MaxIterationsReached,
            StoppingConditionsTooStringent,         // not clear what this means, *reduce* limits?
            Cancelled,

            ERROR_UNKNOWN_CODE
        }

        public TerminationCondition Status { get; private set; }

        public RelaxerStepper_CG(Graph g, GeneratorConfig c)
            : this(g, c, c.IntermediateRelaxationMoveTarget)
        {
        }

        public RelaxerStepper_CG(Graph g, GeneratorConfig c, float move_target_override)
        {
            Graph = g;
            m_config = c;
            MoveTarget = move_target_override;
        }

        private void SetUp()
        {
            m_nodes = Graph.GetAllNodes();
            m_edges = Graph.GetAllEdges();
            m_extra_forces =
                m_nodes
                    .Select(x => x.Parent)
                    .Where(x => x != null)
                    .Distinct()
                    .SelectMany(x => x.GetExtraForces())
                    // we may have removed one end of this force, in which case ignore it
                    // but we won't delete it, as we may undo some graph edits
                    // and it would be a pain to make that also remove and re-add relatively rare
                    // extra forces
                    .Where(x => Graph.Contains(x.N1) && Graph.Contains(x.N2))
                    .ToList();

            // these are shortest path lengths through the graph
            //
            // irrespective of node <-> node or node <-> edge forces, we don't want to be pushed further than this
            // so we shorten the distances of those so they don't stretch edges too far
            //
            // (node <-> node and node <-> edge forces have to be stronger than edge forces
            // as we rely on edges stretching (in other cases) to tell ue when we need to
            // lengthen an edge (inserting a corner)
            m_node_dists = new ShortestPathFinder();

            m_node_dists.FindPathLengths(Graph, x => (x.MaxLength + x.MinLength) / 2);

            int num_pars = m_nodes.Count * 2;

            var temp = new List<double>();

            int p_num = 0;

            foreach (IRelaxationParamSource irps in m_nodes)
            {
                m_source2pars_idx[irps] = p_num;

                p_num += irps.GetParams(temp, p_num);
            }

            m_pars = temp.ToArray();

            alglib.mincgcreatef(m_pars, 1e-4, out opt_state);
            alglib.mincgsuggeststep(opt_state, 1);
            alglib.mincgsetcond(opt_state, 0, 0,
                MoveTarget,
                MaxIterationsPerStep);
            alglib.mincgsetxrep(opt_state, true);

#if DEBUG
            alglib.mincgoptguardsmoothness(opt_state);
#endif
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            if (!m_IsSetup)
            {
                SetUp();

                m_IsSetup = true;

                // should not come in with crossing edges
                if (GraphUtil.AnyCrossingEdges(m_edges))
                {
                    return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                          null, "Found crossing edges in starting geometry.");
                }

            }

            alglib.mincgoptimize(opt_state, EnergyFunc, ReportFunc, null);

            alglib.mincgresults(opt_state, out m_pars, out alglib.mincgreport report);
            // optimiser seems restart from the original pars, unless we explicitly tell it to restart from
            // it's current end position
            alglib.mincgrestartfrom(opt_state, m_pars);

            Status = IntToTC(report.terminationtype);
            Assertion.Assert(Status != TerminationCondition.InfOrNanError);
            Assertion.Assert(Status != TerminationCondition.GradientVerificationError);
            // the problem with this one is I have no idea what it means
            Assertion.Assert(Status != TerminationCondition.StoppingConditionsTooStringent);

            m_energy_count += report.nfev;
            m_iterations += report.iterationscount;

#if DEBUG
            alglib.mincgoptguardresults(opt_state, out alglib.optguardreport ogrep);

            // I *believe* my energies are c1 continuous (they are piecewise functions of the form
            // d > d0 : 0
            //        : (d - d0)^2
            //
            // (as at the joint d - d0 = 0 and has a slope of zero, which should match happily???
            //
            // but I occasionally see "c1: false" from this, possibly a false alarm...?
            //
            // ? possibly only in unit-tests though?
            //
            // oh, wait...  it is discontinuous as we pass a separation of zero
            // well that is probably OK as practically, we never want to be anywhere near that
            System.Diagnostics.Debug.WriteLine($"c0: {!ogrep.nonc0suspected} c1: {!ogrep.nonc1suspected}");
#endif

            if (GraphUtil.AnyCrossingEdges(m_edges))
            {
                return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                      null, "Generated crossing edges during relaxation.");
            }

            TerminationCondition[] convergence_conditions =
                new TerminationCondition[] {
                    TerminationCondition.FunctionLimitReached,
                    TerminationCondition.GradientLimitReached,
                    TerminationCondition.ParameterStepLimitReached,
                    TerminationCondition.MaxIterationsReached
                };

            if (!convergence_conditions.Contains(Status))
            {
                // we only expect max-iter or one of the limits (dF, gradient, dX) as termination conditions

                // not yet clear whether we ever expect this, let's try to bring them to my attention for the moment...
                Assertion.Assert(false);

                return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                  null,
                  $"CG-optimiser unexpected status: {Status}");
            }

            int p_num = 0;

            // do this even for intermediate states, to allow display
            foreach (var n in m_nodes)
            {
                p_num += n.SetParams(m_pars, p_num);
            }

            if (Status == TerminationCondition.MaxIterationsReached)
            {
                return new StepperController.StatusReportInner(StepperController.Status.Iterate,
                  null,
                  "Not yet converged");
            }

            return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                  null,
                  $"Relaxation converged in {m_iterations} ({m_energy_count} function evaluations)"
                  //" move:" + maxd +
                  //" time step:" + step +
                  //" force:" + maxf +
                  //" max edge stretch:" + max_edge_stretch +
                  //" max edge squeeze: " + max_edge_squeeze +
                  //" max edge side squeeze: " + max_edge_side_squeeze +
                  //" max node squeeze: " + max_node_squeeze
                  );
        }

        static double prev = 0;
        static double[] prev_pars;
        private bool m_IsSetup = false;

        void ReportFunc(double[] pars, double func, object o)
        {
            double diff = func - prev;
            prev = func;

            double dist = 0;
            if (prev_pars != null)
            {
                dist = pars.Zip(prev_pars, ValueTuple.Create).Aggregate(0.0, (x, y) => x + (y.Item1 - y.Item2) * (y.Item1 - y.Item2));
                dist = Math.Sqrt(dist);
            }

            prev_pars = pars.ToArray();

            System.Diagnostics.Debug.WriteLine($"F: {func} delta: {diff} dist: {dist}");
        }

        void EnergyFunc(double[] pars, ref double func, object o)
        {
            func = 0;

            double nn_func = 0;
            double e_func = 0;
            double ne_func = 0;
            double ef_func = 0;

            for (int i = 0; i < m_nodes.Count - 1; i++)
            {
                var n1 = m_nodes[i];
                int n1_idx = m_source2pars_idx[n1];
                Vector2D n1pos = new Vector2D(pars[n1_idx], pars[n1_idx + 1]);

                for( int j = i + 1; j < m_nodes.Count; j++)
                {
                    var n2 = m_nodes[j];
                    int n2_idx = m_source2pars_idx[n2];
                    Vector2D n2pos = new Vector2D(pars[n2_idx], pars[n2_idx + 1]);

                    double dist = (n1pos - n2pos).Magnitude;

                    float adjusted_radius =
                        Mathf.Min(
                            m_node_dists.GetDist(n1, n2),
                            n1.Radius + n2.Radius + m_config.RelaxationMinimumSeparation);

                    nn_func += NodeNodeEnergy(dist, adjusted_radius);
                }
            }

            foreach (var edge in m_edges)
            {
                int start_p_idx = m_source2pars_idx[edge.Start];
                int end_p_idx = m_source2pars_idx[edge.End];

                Vector2D start_pos = new Vector2D(pars[start_p_idx], pars[start_p_idx + 1]);
                Vector2D end_pos = new Vector2D(pars[end_p_idx], pars[end_p_idx + 1]);

                double dist = (start_pos - end_pos).Magnitude;

                e_func += EdgeEnergy(dist, edge.MinLength, edge.MaxLength);
            }

            foreach (var edge in m_edges)
            {
                int start_p_idx = m_source2pars_idx[edge.Start];
                int end_p_idx = m_source2pars_idx[edge.End];

                Vector2D start_pos = new Vector2D(pars[start_p_idx], pars[start_p_idx + 1]);
                Vector2D end_pos = new Vector2D(pars[end_p_idx], pars[end_p_idx + 1]);

                for (int i = 0; i < m_nodes.Count; i++)
                {
                    var n1 = m_nodes[i];
                    Vector2D n1pos = new Vector2D(pars[i * 2], pars[i * 2 + 1]);

                    if (!edge.Connects(n1))
                    {
                        double dist = Util.NodeEdgeDist(n1pos, start_pos, end_pos);

                        double summed_radii =
                            Math.Min(m_node_dists.GetDist(edge.Start, n1),
                                     m_node_dists.GetDist(edge.End, n1));

                        double effective_radius =
                            Math.Min(summed_radii,
                                     n1.Radius + edge.HalfWidth) + m_config.RelaxationMinimumSeparation;

                        ne_func += NodeEdgeEnergy(dist, effective_radius);
                    }
                }
            }

            foreach(var f in m_extra_forces)
            {
                int n1_idx = m_source2pars_idx[f.N1];
                int n2_idx = m_source2pars_idx[f.N2];

                Vector2D n1pos = new Vector2D(pars[n1_idx], pars[n1_idx + 1]);
                Vector2D n2pos = new Vector2D(pars[n2_idx], pars[n2_idx + 1]);

                double dist = (n1pos - n2pos).Magnitude;

                ef_func += f.CalcEnergy(dist);
            }

            func =
                nn_func * m_config.NodeToNodeForceScale
                + e_func * m_config.EdgeLengthForceScale
                + ne_func * m_config.EdgeToNodeForceScale
                + ef_func * m_config.ExtraForceScale;
        }

        public static double NodeNodeEnergy(double d, double d_min)
        {
            // outside the minimum is zero penulty
            // zero d_min implies anything goes
            if (d_min == 0 || d >= d_min)
            {
                return 0;
            }

            return (d_min - d) * (d_min - d);
        }

        public static double EdgeEnergy(double d, double d_min, double d_max)
        {
            Assertion.Assert(d_min <= d_max);

            // very similar to node-node energy except the equation is
            // piecewise to allow for the allowed length range

            if (d >= d_min && d <= d_max)
            {
                return 0;
            }

            if (d < d_min)
            {
                return (d_min - d) * (d_min - d);
            }

            return (d_max - d) * (d_max - d);
        }

        public static double NodeEdgeEnergy(double d, double d_min)
        {
            // outside the minimum attracts no penulty
            // d_min of zero turns this off completely (at least in unit-tests, may never happen elsewhere)
            if (d >= d_min)
            {
                return 0;
            }

            // for the moment same form as node<->node energy
            return (d_min - d) * (d_min - d);
        }

        TerminationCondition IntToTC(int code)
        {
            switch (code)
            {
                case -8:
                    return TerminationCondition.InfOrNanError;

                case -7:
                    return TerminationCondition.GradientVerificationError;

                case 1:
                    return TerminationCondition.FunctionLimitReached;

                case 2:
                    return TerminationCondition.ParameterStepLimitReached;

                case 4:
                    return TerminationCondition.GradientLimitReached;

                case 5:
                    return TerminationCondition.MaxIterationsReached;

                case 7:
                    return TerminationCondition.StoppingConditionsTooStringent;

                case 8:
                    return TerminationCondition.Cancelled;
            }

            Assertion.Assert(false);

            return TerminationCondition.ERROR_UNKNOWN_CODE;
        }
    }
}
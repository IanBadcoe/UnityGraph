using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.Gen
{
    internal class RelaxerStepperFactory : IRelaxerFactory
    {
        public IStepper MakeRelaxer(IoCContainer ioc_container, Graph g, GeneratorConfig c)
        {
            return new RelaxerStepper(ioc_container, g, c);
        }
    }

    public class RelaxerStepper : IStepper
    {
        private readonly IoCContainer ioc_container;
        private readonly Graph m_graph;
        private readonly GeneratorConfig m_config;

        private List<INode> m_nodes;
        private List<DirectedEdge> m_edges;

        // whichever is smaller out of the summed-radii and the
        // shortest path through the graph between two nodes
        // we use this as d0 in the node <-> node force function
        // because otherwise a large node can force its second-closest
        // neighbour (and further) so far away that the edge gets split
        // and then the new second-closest neighbour is in the same position
        private ShortestPathFinder m_node_dists;

        private bool m_setup_done = false;

        public RelaxerStepper(IoCContainer ioc_container, Graph g, GeneratorConfig c)
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

            m_setup_done = true;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            if (!m_setup_done)
            {
                SetUp();
            }

            return RelaxStep();
        }

        // step is scaled so that the max force we see causes a movement of max_move
        // until that means a step of > 1, then we start letting the system slow down :-)
        private StepperController.StatusReportInner RelaxStep()
        {
            float maxf = 0.0f;

            m_nodes.ForEach(n => n.Force = new Vector2(0, 0));

            float max_edge_stretch = 1.0f;
            float max_edge_squeeze = 1.0f;

            foreach (DirectedEdge e in m_edges)
            {
                float ratio = AddEdgeForces(e, e.MinLength, e.MaxLength);
                max_edge_stretch = Mathf.Max(ratio, max_edge_stretch);
                max_edge_squeeze = Mathf.Min(ratio, max_edge_squeeze);
            }

            float max_edge_side_squeeze = 0.0f;

            foreach (DirectedEdge e in m_edges)
            {
                foreach (INode n in m_nodes)
                {
                    if (!e.Connects(n))
                    {
                        float ratio = AddNodeEdgeForces(e, n);
                        max_edge_side_squeeze = Mathf.Max(ratio, max_edge_side_squeeze);
                    }
                }
            }

            float max_node_squeeze = 0.0f;

            foreach (INode n in m_nodes)
            {
                foreach (INode m in m_nodes)
                {
                    if (n == m)
                        break;

                    if (!n.Connects(m))
                    {
                        float fraction = AddNodeForces(n, m);

                        // fraction too close, if any...
                        max_node_squeeze = Mathf.Max(max_node_squeeze, 1 - fraction);
                    }
                }
            }

            foreach (INode n in m_nodes)
            {
                maxf = Mathf.Max(n.Force.magnitude, maxf);
            }

            bool ended = true;
            float maxd = 0.0f;
            float step = 0.0f;

            if (maxf > 0)
            {
                step = Mathf.Min(m_config.RelaxationMaxMove / maxf, m_config.RelaxationMaxMove);

                foreach (INode n in m_nodes) 
                {
                    maxd = Mathf.Max(n.Step(step), maxd);
                }

                ended = maxd < m_config.RelaxationMoveTarget && maxf < m_config.RelaxationForceTarget;
            }

            if (GraphUtil.AnyCrossingEdges(m_edges))
            {
                return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                      null, "Generated crossing edges during relaxation.");
            }
            else if (ended)
            {
                return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                      null, "Relaxed to still-point tolerances.");
            }

            return new StepperController.StatusReportInner(StepperController.Status.Iterate,
                  null,
                  " move:" + maxd +
                  " time step:" + step +
                  " force:" + maxf +
                  " max edge stretch:" + max_edge_stretch +
                  " max edge squeeze: " + max_edge_squeeze +
                  " max edge side squeeze: " + max_edge_side_squeeze +
                  " max node squeeze: " + max_node_squeeze);
        }


        // returns the edge length as a fraction of d0
        private float AddEdgeForces(DirectedEdge e, float dmin, float dmax)
        {
            Assertion.Assert(dmin <= dmax);

            INode nStart = e.Start;
            INode nEnd = e.End;

            Vector2 d = nEnd.Position - nStart.Position;

            float l = d.magnitude;

            // in this case can just ignore these as we hope (i) won't happen and (ii) there will be other non-zero
            // forces to pull them apart
            if (l == 0.0f)
                return 1.0f;

            d = d / l;

            ForceReturn fd = UnitEdgeForce(l, dmin, dmax);

            float ratio = fd.Ratio;
            float force = fd.Force * m_config.EdgeLengthForceScale;

            Vector2 f = d * force;
            nStart.Force += f;
            nEnd.Force -= f;

            /*      if (notes != null)
                  {
                     notes.add(new Annotation(nStart.getPos(), nEnd.getPos(), 128, 128, 255,
                           String.format("%6.4f\n%6.4f", force, ratio)));
                  } */

            return ratio;
        }

        // returns separation as a fraction of summed_radii
        private float AddNodeForces(INode node1, INode node2)
        {
            Vector2 d = node2.Position - node1.Position;
            float adjusted_radius =
                Mathf.Min(
                    m_node_dists.GetDist(node1, node2),
                    node1.Radius + node2.Radius + m_config.RelaxationMinimumSeparation);

            float l = d.magnitude;

            // in this case can just ignore these as we hope (i) won't happen and (ii) there will be other non-zero
            // forces to pull them apart
            if (l == 0.0f)
                return 0.0f;

            d = d / l;

            ForceReturn fd = UnitNodeForce(l, adjusted_radius);

            float ratio = fd.Ratio;

            if (ratio != 0)
            {
                float force = fd.Force * m_config.NodeToNodeForceScale;

                Vector2 f = d * force;
                node1.Force += f;
                node2.Force -= f;

                /*         if (notes != null)
                         {
                            notes.add(new Annotation(node1.getPos(), node2.getPos(), 255, 128, 128,
                                  String.format("%6.4f\n%6.4f", force, 1 - ratio)));
                         } */
            }

            return ratio;
        }

        private float AddNodeEdgeForces(DirectedEdge e, INode n)
        {
            Util.NEDRet vals = Util.NodeEdgeDistDetailed(n.Position, e.Start.Position, e.End.Position);

            if (vals == null)
                return 1.0f;

            // our minimum separation is radius1 + radius2 + minimum_separation
            // except where there is a shorter path through the edges
            float effective_summed_radii =
                Mathf.Min(m_node_dists.GetDist(e.Start, n),
                          Mathf.Min(m_node_dists.GetDist(e.End, n),
                                    n.Radius + e.HalfWidth + m_config.RelaxationMinimumSeparation));

            if (vals.Dist > effective_summed_radii)
            {
                return 1.0f;
            }

            float ratio = vals.Dist / effective_summed_radii;

            float force = (ratio - 1) * m_config.EdgeToNodeForceScale;

            Vector2 f = vals.Direction * force;

            n.Force += f;
            // the divide by two seems to be important, otherwise we can add "momentum" to the system and it can spin without ever converging
            f = f / -2;
            e.Start.Force += f;
            e.End.Force += f;

            return ratio;
        }

        struct ForceReturn
        {
            public ForceReturn(float ratio, float force)
            {
                Ratio = ratio;
                Force = force;
            }

            public readonly float Force;
            public readonly float Ratio;
        }

        /**
         * Calculate the force and distortion of an edge constrained to be between dmin and dmax in length.
         *
         * @param l    the current length of the edge
         * @param dmin the minimum permitted length of the edge
         * @param dmax the maximum permitted length of the edge
         * @return a pair of floats, the first is the fractional distortion of the edge.  If between dmin and dmax this
         * is 1.0 (no distortion) if shorter than dmin this is l as a fraction of dmin (< 1.0) and if
         * if longer than dmax then this is l as a fraction of dmax )e.g. > 1.0)
         * <p>
         * The second float is the force.  The sign of the force is that -ve is repulsive (happens when too close)
         * and vice versa.
         */
        ForceReturn UnitEdgeForce(float l, float dmin, float dmax)
        {
            float ratio;

            // between min and max there is no force and we always return 1.0
            if (l < dmin)
            {
                ratio = l / dmin;
            }
            else if (l > dmax)
            {
                ratio = l / dmax;
            }
            else
            {
                ratio = 1.0f;
            }

            float force = (ratio - 1);

            return new ForceReturn(ratio, force);
        }

        /**
         * Calculate force and distance ratio of two circular nodes
         * @param l node separation
         * @param summed_radii ideal minimum separation
         * @return a pair of floats, the first is a fractional measure of how much too close the nodes are,
         * zero if they are more than their summed_radii apart.
         * <p>
         * The second float is the force.  The sign of the force is that -ve is repulsive (happens when too close)
         * the are no attractive forces for nodes so the force is never > 0.
         */
        ForceReturn UnitNodeForce(float l, float summed_radii)
        {
            float ratio = l / summed_radii;

            // no attractive forces
            if (ratio > 1)
            {
                return new ForceReturn(0.0f, 0.0f);
            }

            float force = (ratio - 1);

            // at the moment the relationship between force and overlap is trivial
            // but will keep the two return values in case the force develops a squared term or something...
            return new ForceReturn(-force, force);
        }
    }
}
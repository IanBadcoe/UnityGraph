using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.Stepping;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.Gen
{
    public class EdgeAdjusterStepper : IStepper
    {
        public Graph Graph { get; private set; }

        private readonly GeneratorConfig m_config;

        public EdgeAdjusterStepper(Graph graph, GeneratorConfig config)
        {
            Graph = graph;
            m_config = config;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            switch (status)
            {
                case StepperController.Status.StepIn:
                    while (SplitEdge())
                        ;

                    // tried leaving relaxation of these for a later relax step, which we will have anyway
                    // but that seemed to take longer...
                    IStepper child = new RelaxerStepper_CG(Graph, m_config);

                    return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                          child, "Relaxing split edge.");

                case StepperController.Status.StepOutSuccess:
                    return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                          null, "Successfully relaxed split edge.");

                case StepperController.Status.StepOutFailure:
                    return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                          null, "Failed to relax split edge.");
            }

            // shouldn't get here, crash horribly

            throw new System.NotSupportedException();
        }

        // only stresses above 50% are considered
        // (min length of an edge is 75% max length
        //  so > 150% stressed means we can half it and definitely be zero stress)
        private DirectedEdge MostStressedEdge(List<DirectedEdge> edges)
        {
            float max_stress = 1.1f;
            DirectedEdge ret = null;

            foreach (DirectedEdge e in edges)
            {
                float stress = e.Length() / e.MaxLength;

                if (stress > max_stress)
                {
                    ret = e;
                    max_stress = stress;
                }
            }

            return ret;
        }

        private bool SplitEdge()
        {
            DirectedEdge e = MostStressedEdge(Graph.GetAllEdges());

            if (e == null)
            {
                return false;
            }

            INode c = Graph.AddNode("c", "",
                e.HalfWidth, CircularGeomLayout.Instance);

            Vector2 mid = (e.Start.Position + e.End.Position) / 2;

            c.Position = mid;

            Graph.Disconnect(e.Start, e.End);
            // idea of lengths is to force no more length but allow
            // a longer corridor if required
            DirectedEdge de1 = Graph.Connect(e.Start, c, e.MaxLength, e.HalfWidth,
                CorridorLayout.Instance);
            DirectedEdge de2 = Graph.Connect(c, e.End, e.MaxLength, e.HalfWidth,
                CorridorLayout.Instance);

            // if we are unambiguously inside some template's output
            // then the new node is also inside that
            // (e.g. if Start and End have the same parent, so will we)
            // otherwise we are randomly assigned to the cluster of one end or the other
            if (m_config.Rand().Nextfloat() > 0.5f)
            {
                c.Parent = e.Start.Parent;
            }
            else
            {
                c.Parent = e.End.Parent;
            }

            return true;
        }
    }
}
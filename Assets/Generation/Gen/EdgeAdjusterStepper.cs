using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.Stepping;
using UnityEngine;

namespace Assets.Generation.Gen
{
    internal class EdgeAdjusterStepper : IStepper
    {
        public Graph Graph { get; private set; }

        private readonly DirectedEdge m_edge;
        private readonly GeneratorConfig m_config;

        public EdgeAdjusterStepper(Graph graph, DirectedEdge edge, GeneratorConfig config)
        {
            Graph = graph;
            m_edge = edge;
            m_config = config;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            switch (status)
            {
                case StepperController.Status.StepIn:
                    SplitEdge();

                    IStepper child = new RelaxerStepper_CG(Graph, m_config, false);

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

        private void SplitEdge()
        {
            INode c = Graph.AddNode("c", "",
                m_edge.HalfWidth, CircularGeomLayout.Instance);

            Vector2 mid = (m_edge.Start.Position + m_edge.End.Position) / 2;

            c.Position = mid;

            Graph.Disconnect(m_edge.Start, m_edge.End);
            // idea of lengths is to force no more length but allow
            // a longer corridor if required
            DirectedEdge de1 = Graph.Connect(m_edge.Start, c, m_edge.MinLength / 2, m_edge.MaxLength, m_edge.HalfWidth,
                CorridorLayout.Instance);
            DirectedEdge de2 = Graph.Connect(c, m_edge.End, m_edge.MinLength / 2, m_edge.MaxLength, m_edge.HalfWidth,
                CorridorLayout.Instance);

            de1.Colour = m_edge.Colour;
            de2.Colour = m_edge.Colour;
        }
    }
}
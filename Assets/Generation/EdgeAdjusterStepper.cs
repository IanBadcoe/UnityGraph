using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using UnityEngine;

namespace Assets.Generation
{
    internal class EdgeAdjusterStepperFactory : IAdjusterFactory
    {
        public IStepper MakeAdjuster(IoCContainer ioc_container, Graph graph, DirectedEdge edge, GeneratorConfig c)
        {
            return new EdgeAdjusterStepper(ioc_container, graph, edge, c);
        }
    }

    internal class EdgeAdjusterStepper : IStepper
    {
        private readonly Graph m_graph;
        private readonly DirectedEdge m_edge;
        private readonly GeneratorConfig m_config;
        private readonly IoCContainer m_ioc_container;

        public EdgeAdjusterStepper(IoCContainer ioc_container, Graph graph, DirectedEdge edge, GeneratorConfig config)
        {
            m_graph = graph;
            m_edge = edge;
            m_config = config;
            m_ioc_container = ioc_container;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            switch (status)
            {
                case StepperController.Status.StepIn:
                    SplitEdge();

                    IStepper child = m_ioc_container.RelaxerFactory.MakeRelaxer(m_ioc_container, m_graph, m_config);

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
            INode c = m_graph.AddNode("c", "", "EdgeExtend",
                  m_edge.HalfWidth * 2 /*,
                  m_geom_maker */);

            Vector2 mid = (m_edge.Start.Position + m_edge.End.Position) / 2;

            c.Position = mid;

            m_graph.Disconnect(m_edge.Start, m_edge.End);
            // idea of lengths is to force no more length but allow
            // a longer corridor if required
            DirectedEdge de1 = m_graph.Connect(m_edge.Start, c, m_edge.MinLength / 2, m_edge.MaxLength, m_edge.HalfWidth);
            DirectedEdge de2 = m_graph.Connect(c, m_edge.End, m_edge.MinLength / 2, m_edge.MaxLength, m_edge.HalfWidth);

            de1.Colour = m_edge.Colour;
            de2.Colour = m_edge.Colour;
        }
    }
}
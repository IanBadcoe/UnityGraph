using Generation.G;
using Generation.IoC;
using Generation.Stepping;

namespace Generation
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

        public StepperController.StatusReportInner step(StepperController.Status status)
        {
            throw new System.NotImplementedException();
        }
    }
}
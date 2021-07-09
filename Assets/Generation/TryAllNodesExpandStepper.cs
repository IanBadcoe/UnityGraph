using Generation.G;
using Generation.IoC;
using Generation.Stepping;
using Generation.Templates;

namespace Generation
{
    internal class TryAllNodesExpandStepperFactory : IAllNodesExpanderFactory
    {
        public IStepper MakeAllNodesExpander(IoCContainer ioc_container, Graph g, TemplateStore ts, GeneratorConfig c)
        {
            return new TryAllNodesExpandStepper(ioc_container, g, ts, c);
        }
    }

    internal class TryAllNodesExpandStepper : IStepper
    {
        private readonly Graph m_graph;
        private readonly TemplateStore m_templates;
        private readonly GeneratorConfig m_config;
        private readonly IoCContainer m_ioc_container;

        public TryAllNodesExpandStepper(IoCContainer ioc_container, Graph graph, TemplateStore templates, GeneratorConfig config)
        {
            m_ioc_container = ioc_container;
            m_graph = graph;
            m_templates = templates;
            m_config = config;
        }

        public StepperController.StatusReportInner step(StepperController.Status status)
        {
            throw new System.NotImplementedException();
        }
    }
}
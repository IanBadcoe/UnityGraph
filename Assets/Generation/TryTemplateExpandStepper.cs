using Generation.G;
using Generation.IoC;
using Generation.Stepping;
using Generation.Templates;

namespace Generation
{
    internal class TryTemplateExpandStepperFactory : INodeTemplateExpanderFactory
    {
        public IStepper MakeNodeTemplateExpander(IoCContainer ioc_container, Graph g, INode n, Template t, GeneratorConfig c)
        {
            return new TryTemplateExpandStepper(ioc_container, g, n, t, c);
        }
    }

    internal class TryTemplateExpandStepper : IStepper
    {
        public StepperController.StatusReportInner step(StepperController.Status status)
        {
            throw new System.NotImplementedException();
        }

        private readonly Graph m_graph;
        private readonly INode m_node;
        private readonly Template m_template;
        private readonly GeneratorConfig m_config;
        private readonly IoCContainer m_ioc_container;

        public TryTemplateExpandStepper(IoCContainer ioc_container, Graph graph, INode node, Template template, GeneratorConfig config)
        {
            m_ioc_container = ioc_container;
            m_graph = graph;
            m_node = node;
            m_template = template;
            m_config = config;
        }
    }
}
using Generation.G;
using Generation.IoC;
using Generation.Stepping;
using Generation.Templates;
using System.Collections.ObjectModel;

namespace Generation
{
    internal class TryAllTemplatesOnOneNodeStepperFactory : INodeExpanderFactory
    {
        public IStepper MakeNodeExpander(IoCContainer ioc_container, Graph g, INode n, Collection<Template> templates, GeneratorConfig c)
        {
            return new TryAllTemplatesOnOneNodeStepper(ioc_container, g, n, templates, c);
        }
    }

    internal class TryAllTemplatesOnOneNodeStepper : IStepper
    {
        private readonly Graph m_graph;
        private readonly INode m_node;
        private readonly Collection<Template> m_templates;
        private readonly GeneratorConfig m_config;
        private readonly IoCContainer m_ioc_container;

        public TryAllTemplatesOnOneNodeStepper(IoCContainer ioc_container, Graph graph, INode node, Collection<Template> templates, GeneratorConfig config)
        {
            m_ioc_container = ioc_container;
            m_graph = graph;
            m_node = node;
            m_config = config;
            m_templates = templates;
        }

        public StepperController.StatusReportInner step(StepperController.Status status)
        {
            throw new System.NotImplementedException();
        }
    }
}
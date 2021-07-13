using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assets.Generation.U;

namespace Assets.Generation
{
    internal class TryAllTemplatesOnOneNodeStepperFactory : INodeExpanderFactory
    {
        public IStepper MakeNodeExpander(IoCContainer ioc_container, Graph g, INode n, List<Template> templates, GeneratorConfig c)
        {
            return new TryAllTemplatesOnOneNodeStepper(ioc_container, g, n, templates, c);
        }
    }

    internal class TryAllTemplatesOnOneNodeStepper : IStepper
    {
        private readonly Graph m_graph;
        private readonly INode m_node;
        private readonly List<Template> m_templates;
        private readonly GeneratorConfig m_config;
        private readonly IoCContainer m_ioc_container;

        public TryAllTemplatesOnOneNodeStepper(IoCContainer ioc_container, Graph graph, INode node, List<Template> templates, GeneratorConfig config)
        {
            m_ioc_container = ioc_container;
            m_graph = graph;
            m_node = node;
            m_config = config;
            m_templates = templates;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            // if our child succeeds, we succeed
            if (status == StepperController.Status.StepOutSuccess)
            {
                return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                      null, "engine.Graph Expand step Succeeded");
            }

            // no matter what other previous status, if we run out of templates we're a fail
            if (m_templates.Count == 0)
            {
                return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                      null, "engine.Node: " + m_node.Name + " failed to expand");
            }

            Template t = LevelUtil.RemoveRandom(m_config.Rand(), m_templates);

            IStepper child = m_ioc_container.NodeTemplateExpanderFactory.MakeNodeTemplateExpander(
                  m_ioc_container, m_graph, m_node, t, m_config);

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                  child, "Trying to expand node: " + m_node.Name + " with template: " + t.Name);
        }
    }
}
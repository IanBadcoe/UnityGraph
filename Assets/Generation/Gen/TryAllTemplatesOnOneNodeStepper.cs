using Assets.Generation.G;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using Assets.Generation.U;
using System.Collections.Generic;

namespace Assets.Generation.Gen
{
    internal class TryAllTemplatesOnOneNodeStepper : IStepper
    {
        public Graph Graph { get; private set; }

        private readonly INode m_node;
        private readonly List<Template> m_templates;
        private readonly GeneratorConfig m_config;

        public TryAllTemplatesOnOneNodeStepper(Graph graph, INode node, List<Template> templates, GeneratorConfig config)
        {
            Graph = graph;
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

            Template t = Util.RemoveRandom(m_config.Rand(), m_templates);

            IStepper child = new TryTemplateExpandStepper(Graph, m_node, t, m_config);

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                  child, "Trying to expand node: " + m_node.Name + " with template: " + t.Name);
        }
    }
}
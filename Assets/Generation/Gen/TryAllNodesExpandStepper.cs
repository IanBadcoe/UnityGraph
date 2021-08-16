using Assets.Generation.G;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using Assets.Generation.U;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Generation.Gen
{
    public class TryAllNodesExpandStepper : IStepper
    {
        public Graph Graph { get; private set; }
        private readonly TemplateStore m_templates;
        private readonly GeneratorConfig m_config;

        private readonly List<INode> m_all_nodes;

        public TryAllNodesExpandStepper(Graph graph, TemplateStore templates, GeneratorConfig config)
        {
            Graph = graph;
            m_templates = templates;
            m_config = config;

            m_all_nodes = Graph.GetAllNodes().Where(x => x.Codes.Contains("e")).ToList();
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            // if our child succeeds, we succeed
            if (status == StepperController.Status.StepOutSuccess)
            {
                return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                      null, "engine.Graph Expand step Succeeded");
            }

            // no matter what other previous status, if we run out of nodes we're a fail
            if (m_all_nodes.Count == 0)
            {
                return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                      null, "All nodes failed to expand");
            }

            INode node = Util.RemoveRandom<INode>(m_config.Rand(), m_all_nodes);

            List<Template> templates = m_templates.GetTemplatesCopy();

            // if this was our last chance at a node, take only templates that expand further
            // (could also allow those that expand enough, but that would involve copying the
            // required size down here...
            if (m_all_nodes.Count == 0)
            {
                templates = templates.Where(t => t.Codes.Contains("e")).ToList();
            }

            IStepper child = new TryAllTemplatesOnOneNodeStepper(
                  Graph, node, templates, m_config);

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                  child, "Trying to expand node: " + node.Name);
        }
    }
}
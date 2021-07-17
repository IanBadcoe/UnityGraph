﻿using Assets.Generation.U;
using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Assets.Generation.Gen
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

        private readonly List<INode> m_all_nodes;

        public TryAllNodesExpandStepper(IoCContainer ioc_container, Graph graph, TemplateStore templates, GeneratorConfig config)
        {
            m_ioc_container = ioc_container;
            m_graph = graph;
            m_templates = templates;
            m_config = config;

            m_all_nodes = m_graph.GetAllNodes();
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

            INode node = LevelUtil.RemoveRandom<INode>(m_config.Rand(), m_all_nodes);

            List<Template> templates = m_templates.GetTemplatesCopy();

            // if this was our last chance at a node, take only templates that expand further
            // (could also allow those that expand enough, but that would involve copying the
            // required size down here...
            if (m_all_nodes.Count == 0) {
                templates = templates.Where(t => t.Codes.Contains("e")).ToList();
            }

            IStepper child = m_ioc_container.NodeExpanderFactory.MakeNodeExpander(
                  m_ioc_container, m_graph, node, templates, m_config);

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                  child, "Trying to expand node: " + node.Name);
        }
    }
}
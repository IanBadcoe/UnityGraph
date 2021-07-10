using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using System;
using System.Collections.Generic;

namespace Assets.Generation
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
        private readonly Graph m_graph;
        private readonly INode m_node;
        private readonly Template m_template;
        private readonly GeneratorConfig m_config;
        private readonly IoCContainer m_ioc_container;

        private enum Phase
        {
            ExpandRelax,
            EdgeCorrection
        }

        private Phase m_phase = Phase.ExpandRelax;

        public TryTemplateExpandStepper(IoCContainer ioc_container, Graph graph, INode node, Template template, GeneratorConfig config)
        {
            m_ioc_container = ioc_container;
            m_graph = graph;
            m_node = node;
            m_template = template;
            m_config = config;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            if (status == StepperController.Status.StepIn)
            {
                if (m_template.Expand(m_graph, m_node, m_config.Rand()))
                {
                    IStepper child = m_ioc_container.RelaxerFactory.MakeRelaxer(m_ioc_container, m_graph, m_config);

                    return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                          child, "Relaxing successful expansion.");
                }

                return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                      null, "Failed to expand");
            }

            if (m_phase == Phase.ExpandRelax)
            {
                return ExpandRelaxReturn(status);
            }

            return EdgeRelaxReturn(status);
        }

        private StepperController.StatusReportInner ExpandRelaxReturn(StepperController.Status status)
        {
            switch (status)
            {
                // succeeded in relaxing expanded graph,
                // look for a first edge to relax
                case StepperController.Status.StepOutSuccess:
                    m_phase = Phase.EdgeCorrection;

                    StepperController.StatusReportInner ret = TryLaunchEdgeAdjust();

                    if (ret != null)
                    {
                        return ret;
                    }

                    return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                          null, "No stressed edges to adjust");

                case StepperController.Status.StepOutFailure:
                    return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                          null, "Failed to relax expanded node.");
            }

            // should never get here, just try to blow things up

            throw new NotSupportedException();
        }

        private StepperController.StatusReportInner EdgeRelaxReturn(StepperController.Status status)
        {
            switch (status)
            {
                // succeeded in relaxing expanded graph,
                // look for a first edge to relax
                case StepperController.Status.StepOutSuccess:
                    StepperController.StatusReportInner ret = TryLaunchEdgeAdjust();

                    if (ret != null)
                    {
                        return ret;
                    }

                    return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                          null, "No more stressed edges to adjust");

                case StepperController.Status.StepOutFailure:
                    return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                          null, "Failed to adjust edge.");
            }

            // should never get here, just try to blow things up

            throw new NotSupportedException();
        }

        private StepperController.StatusReportInner TryLaunchEdgeAdjust()
        {
            DirectedEdge e = MostStressedEdge(m_graph.GetAllEdges());

            if (e == null)
            {
                return null;
            }

            IStepper child = m_ioc_container.AdjusterFactory.MakeAdjuster(m_ioc_container, m_graph, e, m_config);

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                  child, "Adjusting an edge.");
        }

        // only stresses above 10% are considered
        private DirectedEdge MostStressedEdge(List<DirectedEdge> edges)
        {
            float max_stress = 1.1f;
            DirectedEdge ret = null;

            foreach (DirectedEdge e in edges)
            {
                float stress = e.Length() / e.MaxLength;

                if (stress > max_stress)
                {
                    ret = e;
                    max_stress = stress;
                }
            }

            return ret;
        }
    }
}
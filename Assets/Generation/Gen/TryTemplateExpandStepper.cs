using Assets.Generation.G;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using System;

namespace Assets.Generation.Gen
{
    public class TryTemplateExpandStepper : IStepper
    {
        public Graph Graph { get; private set; }

        private readonly Node m_node;
        private readonly Template m_template;
        private readonly GeneratorConfig m_config;

        private enum Phase
        {
            ExpandRelax,
            EdgeCorrection
        }

        private Phase m_phase = Phase.ExpandRelax;

        public TryTemplateExpandStepper(Graph graph, Node node, Template template, GeneratorConfig config)
        {
            Graph = graph;
            m_node = node;
            m_template = template;
            m_config = config;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            if (status == StepperController.Status.StepIn)
            {
                if (m_template.Expand(Graph, m_node, m_config.Rand()))
                {
                    IStepper child = new RelaxerStepper_CG(Graph, m_config);

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
            IStepper child = new EdgeAdjusterStepper(Graph, m_config);

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                  child, "Adjusting an edge.");
        }
    }
}
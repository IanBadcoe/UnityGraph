﻿using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.Gen
{
    class ExpandToSizeStepper : IStepper
    {
        private readonly Graph m_graph;
        private readonly int m_required_size;
        private readonly TemplateStore m_templates;
        private readonly GeneratorConfig m_config;
        private readonly int m_orig_size;

        private readonly IoCContainer m_ioc_container;

        public ExpandToSizeStepper(IoCContainer m_ioc_container, Graph graph, int required_size, TemplateStore templates,
                                   GeneratorConfig c)
        {
            this.m_ioc_container = m_ioc_container;
            m_graph = graph;
            m_orig_size = m_graph == null ? 0 : m_graph.NumNodes();
            m_required_size = required_size;
            m_templates = templates;
            m_config = c;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            switch (status)
            {
                case StepperController.Status.StepIn:
                case StepperController.Status.StepOutSuccess:
                    if (m_graph.NumNodes() >= m_required_size)
                    {
                        Debug.WriteLine($"Completing expansion steps, current size: {m_graph.NumNodes()}, reached target: {m_required_size}");

                        return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                                null, "Target size reached");
                    }

                    Debug.WriteLine($"Starting expand step, current size: {m_graph.NumNodes()}, target: {m_required_size}");

                    IStepper child = m_ioc_container.AllNodesExpanderFactory.MakeAllNodesExpander(
                            m_ioc_container, m_graph, m_templates, m_config);

                    return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                            child, "More expansion required.");

                case StepperController.Status.StepOutFailure:
                    if (m_graph.NumNodes() > m_orig_size)
                    {
                        return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                                null, "Partial success");
                    }

                    return new StepperController.StatusReportInner(StepperController.Status.StepOutFailure,
                            null, "Failed.");
            }

            throw new NotSupportedException();
        }
    }
}

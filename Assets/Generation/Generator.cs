using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using Assets.Generation.U;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Generation
{
    public class Generator : MonoBehaviour
    {
        public GeneratorConfig Config;

        // need a better way of making and setting these, but while we only have one...
        private TemplateStore Templates = new TemplateStore1();

        private IoCContainer m_ioc_container;

        private UnionHelper m_union_helper;

        public enum Phase
        {
            Init,
            GraphExpand,
            FinalRelax,
            BaseGeometry,
            Union,
            Done
        }

        private Phase m_phase = Phase.Init;

        private Graph m_graph;

        private StepperController m_expander;
        private StepperController m_final_relaxer;

        private int m_reqSize;
        private bool m_complete = false;

        private void Start()
        {
            //UnityEngine.Assertion.Assert.raiseExceptions = true;

            m_ioc_container = new IoCContainer(
                new RelaxerStepperFactory(),
                new TryAllNodesExpandStepperFactory(),
                new TryAllTemplatesOnOneNodeStepperFactory(),
                new TryTemplateExpandStepperFactory(),
                new EdgeAdjusterStepperFactory(),
                new DefaultLayoutFactory()
            );

            m_reqSize = 5;
        }

        private void Update()
        {
            if (!m_complete)
            {
                StepperController.StatusReport ret;

                ret = Step();

                // take before complete so we can draw it...
                //m_level = m_generator.getLevel();

                if (ret == null || ret.Complete)
                {
                    //if (ret.Status != StepperController.Status.StepOutSuccess)
                    //{
                    //    exit();
                    //}

                    m_complete = true;
                }
            }
        }

        public StepperController.StatusReport Step()
        {
            switch (m_phase)
            {
                case Phase.Init:
                    return InitStep();

                case Phase.GraphExpand:
                    return GraphExpandStep();

                case Phase.FinalRelax:
                    return FinalRelaxStep();

                case Phase.BaseGeometry:
                    return BaseGeometryStep();

                case Phase.Union:
                    return UnionStep();

                case Phase.Done:
                    break;
//                    return DoneStep();
            }

            // really shouldn't happen

            return null;
        }

        private StepperController.StatusReport InitStep()
        {
            m_graph = MakeSeed();

            m_expander = new StepperController(m_graph,
                  new ExpandToSizeStepper(m_ioc_container, m_graph, m_reqSize, Templates,
                        Config));

            GeneratorConfig temp = GeneratorConfig.ShallowCopy(Config);
            temp.RelaxationForceTarget /= 5;
            temp.RelaxationMoveTarget /= 5;

            m_final_relaxer = new StepperController(m_graph,
                  new RelaxerStepper(m_ioc_container, m_graph, temp));

            m_phase = Phase.GraphExpand;

            return new StepperController.StatusReport(
                  new StepperController.StatusReportInner(StepperController.Status.Iterate,
                     null, "engine.Level creation initialised"),
                  false);
        }

        private StepperController.StatusReport GraphExpandStep()
        {
            StepperController.StatusReport ret = null;

            for (int i = 0; i < Config.ExpandStepsToRun; i++)
            {
                ret = m_expander.Step();

                if (ret.Complete)
                {
                    m_phase = Phase.FinalRelax;

                    return new StepperController.StatusReport(
                          StepperController.Status.Iterate,
                          ret.Log,
                          false);
                }
            }

            return ret;
        }

        private StepperController.StatusReport FinalRelaxStep()
        {
            StepperController.StatusReport ret = null;

            for (int i = 0; i < Config.ExpandStepsToRun; i++)
            {
                ret = m_final_relaxer.Step();

                if (ret.Complete)
                {
                    m_phase = Phase.BaseGeometry;

                    return new StepperController.StatusReport(
                          StepperController.Status.Iterate,
                          ret.Log,
                          false);
                }
            }

            return ret;
        }

        private StepperController.StatusReport BaseGeometryStep()
        {
            m_union_helper = new UnionHelper();

            m_union_helper.GenerateGeometry(m_graph);

            m_phase = Phase.Union;

            return new StepperController.StatusReport(
                  new StepperController.StatusReportInner(StepperController.Status.Iterate,
                        null, "engine.Level base geometry generated"),
                  false);
        }

        private StepperController.StatusReport UnionStep()
        {
            bool done = m_union_helper.UnionOne(Config.Rand());

            if (done)
            {
                m_phase = Phase.Done;

                return new StepperController.StatusReport(
                      new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                            null, "Geometry merged."),
                      false);
            }

            return new StepperController.StatusReport(
                    new StepperController.StatusReportInner(StepperController.Status.Iterate,
                        null, "Merging geometry"),
                    false);
        }

        private StepperController.StatusReport DoneStep()
        {
//            m_level = m_union_helper.makeLevel(m_config.CellSize, m_config.WallFacetLength);

            m_union_helper = null;

            return new StepperController.StatusReport(
                    StepperController.Status.StepOutSuccess,
                    "engine.Level complete",
                    true);
        }

        private Graph MakeSeed()
        {
            Graph ret = new Graph(m_ioc_container.LayoutFactory);

            INode start = ret.AddNode("Start", "<", "Seed", 55f);
            INode expander = ret.AddNode("engine.StepperController", "e", "Seed", 55f);
            INode end = ret.AddNode("End", ">", "Seed", 55f);

            start.Position = new Vector2(-100, 0);
            expander.Position = new Vector2(0, 0);
            end.Position = new Vector2(0, 100);

            ret.Connect(start, expander, 90, 110, 10);
            ret.Connect(expander, end, 90, 110, 10);

            //not expandable, which simplifies expansion as start won't need replacing
            return ret;
        }
    }
}
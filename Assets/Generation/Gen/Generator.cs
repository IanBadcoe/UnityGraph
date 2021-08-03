using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using UnityEngine;


namespace Assets.Generation.Gen
{
    public class Generator : IStepper
    {
        public Graph Graph { get; private set; }

        public GeneratorConfig Config;

        // only public as this is where we leave the geometry for the moment
        // if we send it somewhere else, won't need it here
        // but for the moment LoopsDrawer finds this here...
        public UnionHelper UnionHelper { get; private set; }

        // need a better way of making and setting these, but while we only have one...
        private readonly TemplateStore Templates = new TemplateStore1();

        private readonly IoCContainer m_ioc_container;

        public enum Phase
        {
            GraphExpand,
            FinalRelax,
            Done
        }

        private Phase m_phase = Phase.GraphExpand;

        public bool Pause { get; set; }

        private readonly int m_reqSize;

        public Generator(Graph graph, int req_size)
        {
            //UnityEngine.Assertion.Assert.raiseExceptions = true;

            Graph = graph;

            m_ioc_container = new IoCContainer(
                new RelaxerStepper_CGFactory(),
                new TryAllNodesExpandStepperFactory(),
                new TryAllTemplatesOnOneNodeStepperFactory(),
                new TryTemplateExpandStepperFactory(),
                new EdgeAdjusterStepperFactory()
            );

            m_reqSize = req_size;
        }

        public StepperController.StatusReportInner Step(StepperController.Status status)
        {
            switch (status)
            {
                case StepperController.Status.StepIn:
                    return Init();

                case StepperController.Status.StepOutSuccess:
                case StepperController.Status.Iterate:
                    switch (m_phase)
                    {
                        case Phase.GraphExpand:
                            return ExpandDone();

                        case Phase.FinalRelax:
                            return FinalRelaxDone();
                    }
                    break;
            }

            // really shouldn't happen

            return null;
        }

        private StepperController.StatusReportInner Init()
        {
            MakeSeed();

            IStepper expander = new ExpandToSizeStepper(m_ioc_container, Graph, m_reqSize, Templates, Config);

            m_phase = Phase.GraphExpand;

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                     expander, "engine.Level creation initialised");
        }

        private StepperController.StatusReportInner ExpandDone()
        {
            IStepper stepper = new RelaxerStepper_CG(m_ioc_container, Graph, Config, true);

            m_phase = Phase.FinalRelax;

            return new StepperController.StatusReportInner(StepperController.Status.StepIn, stepper, "Expansion done");
        }

        private StepperController.StatusReportInner FinalRelaxDone()
        {
            UnionHelper = new UnionHelper();

            UnionHelper.GenerateGeometry(Graph);

            while (!UnionHelper.UnionOne(Config.Rand()))
            {
                ;
            }

            m_phase = Phase.Done;

            return new StepperController.StatusReportInner(StepperController.Status.StepOutSuccess,
                        null, "Geometry merged.");
        }

        private void MakeSeed()
        {
            INode start = Graph.AddNode("Start", "<", "Seed", 55f, CircularGeomLayout.Instance);
            INode expander = Graph.AddNode("engine.StepperController", "e", "Seed", 55f, CircularGeomLayout.Instance);
            INode end = Graph.AddNode("End", ">", "Seed", 55f, CircularGeomLayout.Instance);

            start.Position = new Vector2(0, -100);
            expander.Position = new Vector2(0, 0);
            end.Position = new Vector2(100, 0);

            Graph.Connect(start, expander, 90, 110, 10, CorridorLayout.Instance);
            Graph.Connect(expander, end, 90, 110, 10, CorridorLayout.Instance);
        }
    }
}
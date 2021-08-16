using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using System;
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

        public enum Phase
        {
            GraphExpand,
            FinalRelax,
            FinalEdgeAdjust,
            Done
        }

        private Phase m_phase = Phase.GraphExpand;

        private float CurrFinalRelaxMoveTarget = -1;

        private readonly int m_reqSize;

        public Generator(Graph graph, int req_size)
        {
            //UnityEngine.Assertion.Assert.raiseExceptions = true;

            Graph = graph;

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
                            m_phase = Phase.FinalRelax;

                            return new StepperController.StatusReportInner(StepperController.Status.Iterate,
                                null, "Running final relaxation...");

                        case Phase.FinalRelax:
                            return FinalRelaxStep();

                        case Phase.FinalEdgeAdjust:
                            if (CurrFinalRelaxMoveTarget > Config.FinalRelaxationMoveTarget)
                            {
                                return FinalEdgeAdjustStep();
                            }

                            m_phase = Phase.Done;

                            return new StepperController.StatusReportInner(StepperController.Status.Iterate,
                                null, "Finalising...");

                        case Phase.Done:
                            return Done();
                    }
                    break;
            }

            // really shouldn't happen

            return null;
        }

        private StepperController.StatusReportInner Init()
        {
            MakeSeed();

            IStepper expander = new ExpandToSizeStepper(Graph, m_reqSize, Templates, Config);

            m_phase = Phase.GraphExpand;

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                     expander, "engine.Level creation initialised");
        }

        private StepperController.StatusReportInner FinalRelaxStep()
        {
            if (CurrFinalRelaxMoveTarget == -1)
            {
                CurrFinalRelaxMoveTarget = Config.IntermediateRelaxationMoveTarget;
            }

            CurrFinalRelaxMoveTarget /= 2;

            if (CurrFinalRelaxMoveTarget < Config.FinalRelaxationMoveTarget)
            {
                CurrFinalRelaxMoveTarget = Config.FinalRelaxationMoveTarget;
            }

            m_phase = Phase.FinalEdgeAdjust;

            IStepper stepper = new RelaxerStepper_CG(Graph, Config, CurrFinalRelaxMoveTarget);

            return new StepperController.StatusReportInner(StepperController.Status.StepIn, stepper, $"Relaxing to {CurrFinalRelaxMoveTarget}");
        }

        private StepperController.StatusReportInner FinalEdgeAdjustStep()
        {
            IStepper child = new EdgeAdjusterStepper(Graph, Config);

            // we cycle between adjusting edges and relaxing to tighter criteria
            m_phase = Phase.FinalRelax;

            return new StepperController.StatusReportInner(StepperController.Status.StepIn,
                  child, "Adjusting edges...");
        }

        private StepperController.StatusReportInner Done()
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
            INode start = Graph.AddNode("Start", "<", 3f, CircularGeomLayout.Instance);
            INode expander = Graph.AddNode("engine.StepperController", "e", 1f, CircularGeomLayout.Instance);
            INode end = Graph.AddNode("End", ">", 3f, CircularGeomLayout.Instance);

            start.Position = new Vector2(0, -4);
            expander.Position = new Vector2(0, 0);
            end.Position = new Vector2(4, 0);

            Graph.Connect(start, expander, 4.5f, 1, CorridorLayout.Instance);
            Graph.Connect(expander, end, 4.5f, 1, CorridorLayout.Instance);
        }
    }
}
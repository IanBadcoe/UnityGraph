using Assets.Generation.G;
using Assets.Generation.Gen;
using Assets.Generation.GeomRep;
using Assets.Generation.Stepping;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Behaviour
{
    public class GeneratorTestBehaviour : DataProvider
    {
        public int NodesLimit = 10;
        public GeneratorConfig Config = new GeneratorConfig();

        private Generator m_generator;

        LayerConfigBehaviour LCB;

        private void Awake()
        {
            LCB = Transform.FindObjectOfType<LayerConfigBehaviour>();
        }

        public override IReadOnlyDictionary<string, ILoopSet> GetLoops()
        {
            if (m_generator != null && m_generator.UnionHelper != null)
            {
                return m_generator.UnionHelper.MergedLoops;
            }

            return null;
        }

        public override Graph GetGraph()
        {
            if (m_generator != null)
            {
                return m_generator.Graph;
            }

            return null;
        }

        private void Start()
        {
            Graph graph = new Graph();

            m_generator = new Generator(graph, NodesLimit, LCB)
            {
                Config = Config
            };

            StepperBehaviour sb = gameObject.AddComponent<StepperBehaviour>();

            sb.Controller = new StepperController(m_generator);
        }
    }
}
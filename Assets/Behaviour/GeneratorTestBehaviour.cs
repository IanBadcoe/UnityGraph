using Assets.Generation.G;
using Assets.Generation.Gen;
using Assets.Generation.GeomRep;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using UnityEngine;


namespace Assets.Behaviour
{
    public class GeneratorTestBehaviour : GeneratorProvider
    {
        public GeneratorConfig Config = new GeneratorConfig();
        public int RequiredSize = 15;

        private readonly Generator m_generator;

        public override Generator GetGenerator()
        {
            return m_generator;
        }

        GeneratorTestBehaviour()
        {
            Graph graph = new Graph();

            m_generator = new Generator(graph, RequiredSize);
            m_generator.Config = Config;
        }

        private void Start()
        {
            GameObject go = new GameObject();
            go.transform.parent = transform.parent;
            StepperBehaviour sb = go.AddComponent<StepperBehaviour>();

            sb.Controller = new StepperController(m_generator);
        }
    }
}
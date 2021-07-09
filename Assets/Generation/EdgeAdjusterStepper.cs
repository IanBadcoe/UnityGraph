using Generation.G;
using Generation.IoC;
using Generation.Stepping;

namespace Generation
{
    internal class EdgeAdjusterStepperFactory : IAdjusterFactory
    {
        public IStepper MakeAdjuster(IoCContainer ioc_container, Graph graph, DirectedEdge edge, GeneratorConfig c)
        {
            throw new System.NotImplementedException();
        }
    }
}
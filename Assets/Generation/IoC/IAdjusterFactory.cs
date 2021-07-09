using Assets.Generation.G;
using Assets.Generation.Stepping;

namespace Assets.Generation.IoC
{
    public interface IAdjusterFactory
    {
        IStepper MakeAdjuster(IoCContainer ioc_container,
                              Graph graph, DirectedEdge edge, GeneratorConfig c);
    }
}
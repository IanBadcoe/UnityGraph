using Generation.G;
using Generation.Stepping;

namespace Generation.IoC
{
    public interface IAdjusterFactory
    {
        IStepper MakeAdjuster(IoCContainer ioc_container,
                              Graph graph, DirectedEdge edge, GeneratorConfig c);
    }
}
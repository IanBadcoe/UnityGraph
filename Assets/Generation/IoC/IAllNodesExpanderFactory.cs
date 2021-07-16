using Assets.Generation.G;
using Assets.Generation.Gen;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;

namespace Assets.Generation.IoC
{
    public interface IAllNodesExpanderFactory
    {
        IStepper MakeAllNodesExpander(IoCContainer ioc_container,
                                      Graph g, TemplateStore ts, GeneratorConfig c);
    }
}
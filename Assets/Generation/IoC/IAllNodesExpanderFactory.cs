using Generation.G;
using Generation.Stepping;
using Generation.Templates;

namespace Generation.IoC
{
    public interface IAllNodesExpanderFactory
    {
        IStepper MakeAllNodesExpander(IoCContainer ioc_container,
                                      Graph g, TemplateStore ts, GeneratorConfig c);
    }
}
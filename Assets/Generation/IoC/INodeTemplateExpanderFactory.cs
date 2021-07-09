using Assets.Generation.G;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;

namespace Assets.Generation.IoC
{
    public interface INodeTemplateExpanderFactory
    {
        IStepper MakeNodeTemplateExpander(IoCContainer ioc_container,
              Graph g, INode n, Template t, GeneratorConfig c);
    }
}
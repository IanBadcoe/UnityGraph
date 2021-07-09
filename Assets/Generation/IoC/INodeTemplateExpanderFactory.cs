using Generation.G;
using Generation.Stepping;
using Generation.Templates;

namespace Generation.IoC
{
    public interface INodeTemplateExpanderFactory
    {
        IStepper makeNodeTemplateExpander(IoCContainer ioc_container,
              Graph g, INode n, Template t, GeneratorConfig c);
    }
}
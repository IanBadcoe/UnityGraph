using Generation.G;
using Generation.Stepping;
using System.Collections.ObjectModel;
using Generation.Templates;

namespace Generation.IoC
{
    public interface INodeExpanderFactory
    {
        IStepper makeNodeExpander(IoCContainer ioc_container,
                                  Graph g, INode n, Collection<Template> templates,
                                  GeneratorConfig c);
    }
}
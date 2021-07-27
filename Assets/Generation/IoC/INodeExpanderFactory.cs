using Assets.Generation.G;
using Assets.Generation.Gen;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using System.Collections.Generic;

namespace Assets.Generation.IoC
{
    public interface INodeExpanderFactory
    {
        IStepper MakeNodeExpander(IoCContainer ioc_container,
                                  Graph g, INode n, List<Template> templates,
                                  GeneratorConfig c);
    }
}
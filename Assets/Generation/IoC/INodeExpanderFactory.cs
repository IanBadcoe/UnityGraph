using Generation.G;
using Generation.Stepping;
using System.Collections.ObjectModel;
using Generation.Templates;
using System.Collections.Generic;

namespace Generation.IoC
{
    public interface INodeExpanderFactory
    {
        IStepper MakeNodeExpander(IoCContainer ioc_container,
                                  Graph g, INode n, List<Template> templates,
                                  GeneratorConfig c);
    }
}
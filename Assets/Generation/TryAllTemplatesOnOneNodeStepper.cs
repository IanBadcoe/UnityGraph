using Generation.G;
using Generation.IoC;
using Generation.Stepping;
using Generation.Templates;
using System.Collections.ObjectModel;

namespace Generation
{
    internal class TryAllTemplatesOnOneNodeStepperFactory : INodeExpanderFactory
    {
        public IStepper makeNodeExpander(IoCContainer ioc_container, Graph g, INode n, Collection<Template> templates, GeneratorConfig c)
        {
            throw new System.NotImplementedException();
        }
    }
}